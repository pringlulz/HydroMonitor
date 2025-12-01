using HydroMonitor.Models;
using HydroMonitor.Repository;
using MQTTnet;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Sensor = HydroMonitor.Models.Sensor;
using System.Collections.ObjectModel;


namespace HydroMonitor.Services
{


    public class MQTTService
    {
        private static IMqttClient? _client;
        private static MqttServer? _server;
        private static MqttClientFactory _factory;
        private readonly SensorReadingDAO _readingDAO;
        private readonly SensorDAO _sensorDAO;
        private readonly String ipAddress = "192.168.0.12";

        private static string baseTopic = "hydromon/sensor/";

        static Regex topicSensorRegex = new Regex(@"sensor/(.+?)/", RegexOptions.Compiled);
        //this defines our different handlers for the different types of sensors 

        public MQTTService(SensorDAO sensorDAO, SensorReadingDAO readingDAO)
        {
            _sensorDAO = sensorDAO;
            _readingDAO = readingDAO;
            Open();
            SetupBroker();
            Subscribe(ipAddress);
        }

        public static void Open()
        {
            _factory = new MqttClientFactory();
            _client =  _factory.CreateMqttClient(); //0.9 is the pi, 0.12 is the host machine
        }

        public Task Close()
        {
            if (_client != null )
                _client.Dispose();
            return Task.CompletedTask;
        }


        public async Task SetupBroker()
        {
            var factory = new MqttServerFactory();
            var options = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()                //.WithDefaultEndpointBoundIPAddress(new System.Net.IPAddress([10,0,2,15]))
                .WithDefaultEndpointPort(1883)
                .Build();
            _server = factory.CreateMqttServer(options);

            await _server.StartAsync();
            _server.ValidatingConnectionAsync += ConnectionValidator; //check that the client connecting is allowed
            _server.InterceptingSubscriptionAsync += CheckClientSubscribe; //check that the thing the client is subscribing to is allowed.
            System.Diagnostics.Debug.WriteLine("MQTT Broker is listening.");

            //await PublishHookMessages();

            //_server.
        }

        private async Task ConnectionValidator(ValidatingConnectionEventArgs arg)
        { 
            if (arg.UserName != null)
            { //if they're smart, they've set their username to be their MAC address.
                System.Diagnostics.Debug.WriteLine("Remote End Point from the args: " + arg.RemoteEndPoint);
                String macAddress = ARPService.GetMacForIP(arg.RemoteEndPoint.ToString());
                if (!arg.UserName.Equals(macAddress))
                {
                    arg.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.BadUserNameOrPassword;
                    return;
                }

                //look up the MAC address in the list of sensors.
                var sensor = _sensorDAO.Load(arg.UserName);
                if (sensor.Id == 0)
                { //sensor is not in the list of sensors, reject the connection.
                    arg.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.NotAuthorized;
                    return;
                }
            }
            else
            { //reject
                if (!arg.RemoteEndPoint.ToString().Contains(ipAddress))
                    arg.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.BadUserNameOrPassword; //_server.DisconnectClientAsync(arg.ClientId);
            }
        }

        private async Task CheckClientSubscribe(InterceptingSubscriptionEventArgs arg)//ClientSubscribedTopicEventArgs arg)
        {
            
            if (arg.TopicFilter.Topic.Contains("setup"))
            { //priveleged topic - don't let anyone but the authenticated mac address subscribe to this.
                List<MqttClientStatus> clients = (List<MqttClientStatus>)await _server.GetClientsAsync();

                String macAddress = ARPService.GetMacForIP(clients.Find(c => c.Id.Equals(arg.ClientId)).RemoteEndPoint.ToString());

                //_server.UnsubscribeAsync(arg.ClientId, new Collection<string> {arg.TopicFilter.Topic});
                var sensor = await  _sensorDAO.Load(macAddress);
                if (sensor.SensorId == 0)
                {
                    arg.Response.ReasonCode = MQTTnet.Protocol.MqttSubscribeReasonCode.NotAuthorized;
                }

                //reply with a token
                string clientKey = await SecureStorage.GetAsync(arg.UserName);
                if (clientKey == null)
                {   // there's no encryption key currently set for this client
                    clientKey = Aes.Create().Key.ToString();
                    await SecureStorage.SetAsync(arg.UserName, clientKey);

                    //send the key back
                }
                else
                { //there's already a key for this client, they should have it.
                    
                }

                PublishHookMessage(sensor, clientKey);
            }
            //arg.UserName

            System.Diagnostics.Debug.WriteLine("Client subscrbied to topic" + arg.TopicFilter);
            arg.Response.ReasonCode = default!;
            arg.Response.ReasonString = default!;
        }

        //public async Task PublishHookMessages()
        //{
        //    List<Sensor> sensors = await _sensorDAO.Load();

        //    foreach (var sensor in sensors)
        //    { //create a message on the broker that the client can listen for. This message tells the client which topic to write to.
        //      //This is probably a stupid way to do it - the client can just write to its mac address as the topic probably.
        //      //But, at least this way, the client gets to firmly establish a connection with the broker before broadcasting.
        //        await PublishHookMessage(sensor);
        //    }
        //}


        public async Task UnpublishHookMessage(String macAddress)
        {
            var message = new MqttApplicationMessageBuilder().WithTopic($"hydromon/setup/{macAddress.ToLower()}").WithPayload(Array.Empty<byte>()).WithRetainFlag(false).Build();
            await _server.InjectApplicationMessage(new InjectedMqttApplicationMessage(message)
            {
                SenderClientId = "SenderClientId"
            });
            System.Diagnostics.Debug.WriteLine($"cleared registration for : {macAddress}");
        }
        public async Task PublishHookMessage(Sensor sensor)
        {// generate a key
            string clientKey = await SecureStorage.GetAsync(sensor.macAddress);
            if (clientKey == null)
            {   // there's no encryption key currently set for this client
                clientKey = Aes.Create().Key.ToString();
                await SecureStorage.SetAsync(sensor.macAddress, clientKey);
            }
            await PublishHookMessage(sensor, clientKey);
        }

        public async Task PublishHookMessage(Sensor sensor, String key)
        {
            if (sensor.macAddress == "") { return; } //skip entries with no mac address
            JsonDocument jsonMessage = JsonDocument.Parse($"{{\"target\": \"{sensor.macAddress}\", \"id\": {sensor.SensorId}, \"key\": \"{key.ToString()}\" }}");
            var message = new MqttApplicationMessageBuilder().WithTopic($"hydromon/setup/{sensor.macAddress}").WithPayload(jsonMessage.RootElement.GetRawText()).WithRetainFlag(true).Build();
            await _server.InjectApplicationMessage(new InjectedMqttApplicationMessage(message)
            {
                SenderClientId = "SenderClientId"
            });
            System.Diagnostics.Debug.WriteLine($"sent initial message: {jsonMessage.RootElement.GetRawText()}");
        }

        //10.0.2.15

        public async Task Subscribe(String ipAddress)
        {
            //TODO: get the bridge from the VM working so that we can have this app be the broker.
            //'till then, have to get data from the pi at 192.168.0.9
            //"192.168.0.12" //MQTTnet.Adapter.MqttConnectingFailedException
            var options = new MqttClientOptionsBuilder().WithTcpServer(ipAddress).Build();

            System.Diagnostics.Debug.WriteLine("Starting subscribe...");

            try
            {
                await _client.ConnectAsync(options, CancellationToken.None);
            } catch (MQTTnet.Adapter.MqttConnectingFailedException)
            {
                System.Diagnostics.Debug.WriteLine($"Subscribe failed (broker is probably not running on target TCP server)...");
                throw;
            }

            if (_client.IsConnected == false)
            {
                throw new Exception("Client is not connected! connect client first.");
            }

            //TODO: subscribe to specific topics per sensor.
            var topicFilter = _factory.CreateTopicFilterBuilder().WithTopic("hydromon/sensor/#").WithAtLeastOnceQoS();

            //get list of sensors from database

            //build new topic filter for each sensor 

            var subscribeOptions = _factory.CreateSubscribeOptionsBuilder().WithTopicFilter(topicFilter).Build();

            var response = await _client.SubscribeAsync(subscribeOptions, CancellationToken.None);

            System.Diagnostics.Debug.WriteLine("MQTT Client subscribed to topic.");

            System.Diagnostics.Debug.WriteLine(response.Items.Count);
            System.Diagnostics.Debug.WriteLine(response.Items.ElementAt(0).ResultCode);
            System.Diagnostics.Debug.WriteLine(response.Items.ElementAt(0).TopicFilter);

            //this is the response handler
            _client.ApplicationMessageReceivedAsync += async m =>
            {
                var matches = topicSensorRegex.Match(m.ApplicationMessage.Topic);
                System.Diagnostics.Debug.WriteLine(m.ApplicationMessage.Topic);
                int sensorId = 0;
                if (matches.Groups.Count != 0)
                {
                    try
                    {
                        sensorId = Convert.ToInt32(matches.Groups[1].Captures[0].Value);
                    } catch
                    {
                        System.Diagnostics.Debug.WriteLine("Unable to parse sensor ID.");
                    }
                }

                if (sensorId != 0)
                {
                    //TODO: what if a mac address is able to report multiple sensors?

                    Sensor sensor = await _sensorDAO.Load(sensorId); //check that topic matches the value we're expecting?
                    //if we can't get a valid sensor back, should we continue?
                    if (sensor.SensorId != 0)
                    parseJSONMessage(m.ApplicationMessage.ConvertPayloadToString(), m.ApplicationMessage.Topic, sensor);
                }

                System.Diagnostics.Debug.WriteLine($"Received Message: {m.ApplicationMessage.ConvertPayloadToString()}");
                //return Task.CompletedTask;
            };

        }


        public Boolean parseJSONMessage(String payloadString, String topic, Sensor sensor)
        {
            JsonDocument jsonMessage = JsonDocument.Parse(payloadString);
            JsonElement readingElem;
            JsonElement timestampElem;
            JsonElement valueElem;
            if (jsonMessage.RootElement.TryGetProperty(Encoding.ASCII.GetBytes("reading").AsSpan(), out readingElem))
            {
                readingElem.TryGetProperty(Encoding.ASCII.GetBytes("timestamp").AsSpan(), out timestampElem);
                readingElem.TryGetProperty(Encoding.ASCII.GetBytes("value").AsSpan(), out valueElem);

                //check that the topic matches
                if (!topic.ToLower().EndsWith(sensor.SensorType.Name.ToLower()))
                { //discard the message, it's not for the topic we're looking for
                    return true;
                }


                System.Diagnostics.Debug.WriteLine(sensor.SensorType.Name);
                switch (sensor.SensorType.Name)
                {
                    case "Temperature": //valueElem extract needs to match source datatype for the JSON, convert to string after
                        SaveToDatabase(valueElem.GetInt32().ToString(), sensor.SensorId, timestampElem.GetDateTime());
                        System.Diagnostics.Debug.WriteLine($"Received temperature message for sensor {sensor.SensorId}");
                        break;
                    case "Humidity": //should use a better enum for this

                        SaveToDatabase(valueElem.GetInt32().ToString() + "%", sensor.SensorId, timestampElem.GetDateTime()); //TODO: percentage
                        System.Diagnostics.Debug.WriteLine($"Saved humidity message for sensor {sensor.SensorId}");
                        break;
                    default:

                        break;
                }
            } else //can't parse it
            {
                return false;
            }
                return true;
        }


        public async void SaveToDatabase(String value, int sensorId, DateTime timestamp)
        {
            SensorReading reading = new SensorReading();
            reading.Timestamp = timestamp;
            reading.rawValue = value;
            //look up the sensor name 

            reading.SensorId = sensorId;

            await _readingDAO.Save(reading);

        }


    }
}
