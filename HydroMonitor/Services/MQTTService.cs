using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HydroMonitor.Models;
using HydroMonitor.Repository;
using MQTTnet;
using MQTTnet.Server;


namespace HydroMonitor.Services
{
    

    public class MQTTService
    {
        private static IMqttClient? _client;
        private static MqttServer? _server;
        private static SensorReadingDAO readingDAO;

        private static string baseTopic = "hydromon/sensor/";
        
        static Regex topicSensorRegex = new Regex(@"sensor/(.+?)/", RegexOptions.Compiled);
        //this defines our different handlers for the different types of sensors 


        public MQTTService()
        {
            OpenAndSubscribe();
            SetupBroker();
        }

        public static async Task SetupBroker()
        {
            var factory = new MqttServerFactory();
            var options = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()                //.WithDefaultEndpointBoundIPAddress(new System.Net.IPAddress([10,0,2,15]))
                .WithDefaultEndpointPort(1883)
                .Build();
            _server = factory.CreateMqttServer(options);
            
            await _server.StartAsync();
            _server.ClientSubscribedTopicAsync += m =>
            {
                System.Diagnostics.Debug.WriteLine("Client subscrbied to topic" + m.TopicFilter);
                return Task.CompletedTask;
            };
            System.Diagnostics.Debug.WriteLine("MQTT Broker is listening.");

            await PublishHookMessages();

            //_server.
        }

        public static async Task PublishHookMessages()
        {
            using (SensorDAO sensorDAO = new SensorDAO())
            {
                List<Sensor> sensors = await sensorDAO.Load();

                foreach (var sensor in sensors)
                { //create a message on the broker that the client can listen for. This message tells the client which topic to write to.
                    //This is probably a stupid way to do it - the client can just write to its mac address as the topic probably.
                    //But, at least this way, the client gets to firmly establish a connection with the broker before broadcasting.
                     PublishHookMessage(sensor);
                }



            }
        }


        public static async Task UnpublishHookMessage(String macAddress)
        {
            var message = new MqttApplicationMessageBuilder().WithTopic($"hydromon/setup/{macAddress.ToLower()}").WithPayload(Array.Empty<byte>()).WithRetainFlag(false).Build();
            await _server.InjectApplicationMessage(new InjectedMqttApplicationMessage(message)
            { 
                SenderClientId = "SenderClientId"
            });
            System.Diagnostics.Debug.WriteLine($"cleared registration for : {macAddress}");
        }

        public static async Task PublishHookMessage(Sensor sensor)
        {
            if (sensor.macAddress == "") { return; } //skip entries with no mac address
            JsonDocument jsonMessage = JsonDocument.Parse($"{{\"target\": \"{sensor.macAddress}\", \"id\": {sensor.SensorId}}}");
            var message = new MqttApplicationMessageBuilder().WithTopic($"hydromon/setup/{sensor.macAddress}").WithPayload(jsonMessage.RootElement.GetRawText()).WithRetainFlag(true).Build();
            await _server.InjectApplicationMessage(new InjectedMqttApplicationMessage(message)
            {
                SenderClientId = "SenderClientId"
            });
            System.Diagnostics.Debug.WriteLine($"sent initial message: {jsonMessage.RootElement.GetRawText()}");
        }






        //10.0.2.15

        public static async Task OpenAndSubscribe()
        {
            System.Diagnostics.Debug.WriteLine("Starting subscribe...");
            var factory = new MqttClientFactory();

            var mqttClient = factory.CreateMqttClient(); //0.9 is the pi, 0.3 is the host machine
            //TODO: get the bridge from the VM working so that we can have this app be the broker.
            //'till then, have to get data from the pi at 192.168.0.9

            var options = new MqttClientOptionsBuilder().WithTcpServer("192.168.0.12").Build();
            //MQTTnet.Adapter.MqttConnectingFailedException
            try
            {
                await mqttClient.ConnectAsync(options, CancellationToken.None);
            }
            catch (MQTTnet.Adapter.MqttConnectingFailedException)
            {
                System.Diagnostics.Debug.WriteLine($"Subscribe failed (broker is probably not running on target TCP server)...");
                throw;
            }
            
            //TODO: subscribe to specific topics per sensor.
            var topicFilter = factory.CreateTopicFilterBuilder().WithTopic("hydromon/sensor/#").WithAtLeastOnceQoS();

            //get list of sensors from database

            //build new topic filter for each sensor

            
            

            var subscribeOptions = factory.CreateSubscribeOptionsBuilder().WithTopicFilter(topicFilter).Build();

            var response = await mqttClient.SubscribeAsync(subscribeOptions, CancellationToken.None);

            System.Diagnostics.Debug.WriteLine("MQTT Client subscribed to topic.");

            System.Diagnostics.Debug.WriteLine(response.Items.Count);
            System.Diagnostics.Debug.WriteLine(response.Items.ElementAt(0).ResultCode );
            System.Diagnostics.Debug.WriteLine(response.Items.ElementAt(0).TopicFilter);

            //this is the response handler
            mqttClient.ApplicationMessageReceivedAsync += m =>
            {
                var matches = topicSensorRegex.Match(m.ApplicationMessage.Topic);
                System.Diagnostics.Debug.WriteLine(m.ApplicationMessage.Topic);
                int sensorId = 0;
                if (matches.Groups.Count != 0 )
                {
                    sensorId = Convert.ToInt32(matches.Groups[1].Captures[0].Value);
                }

                if (sensorId != 0)
                {

                    using (SensorDAO sensorDAO = new SensorDAO()) {

                        Sensor sensor = sensorDAO.Load(sensorId);

                        JsonDocument jsonMessage = JsonDocument.Parse(m.ApplicationMessage.ConvertPayloadToString());
                        JsonElement readingElem;
                        JsonElement timestampElem;
                        JsonElement valueElem;
                        if (jsonMessage.RootElement.TryGetProperty(Encoding.ASCII.GetBytes("reading").AsSpan(), out readingElem)) { 
                            readingElem.TryGetProperty(Encoding.ASCII.GetBytes("timestamp").AsSpan(), out timestampElem);
                            readingElem.TryGetProperty(Encoding.ASCII.GetBytes("value").AsSpan(), out valueElem);
                            System.Diagnostics.Debug.WriteLine(sensor.SensorType.Name);
                            switch (sensor.SensorType.Name) {
                                case "Temperature":
                                    SaveToDatabase(valueElem.GetString(), sensorId, timestampElem.GetDateTime());
                                    System.Diagnostics.Debug.WriteLine($"Received temperature message for sensor {sensorId}");
                                    break;
                                case "Humidity": //should use a better enum for this
                                    SaveToDatabase(valueElem.GetString() + "%", sensorId, timestampElem.GetDateTime()); //TODO: percentage
                                    System.Diagnostics.Debug.WriteLine($"Saved humidity message for sensor {sensorId}");
                                    break;
                                default:

                                    break;
                            }
                        }
                    }
                }
                System.Diagnostics.Debug.WriteLine($"Received Message: {m.ApplicationMessage.ConvertPayloadToString()}");
                return Task.CompletedTask;
            };
            _client = mqttClient;
        }


        //public static async Task setupSensor(String macAddress)
        //{
        //    _client.

        //}


        public static async void SaveToDatabase(Object value, int sensorId, DateTime timestamp)
        {
            SensorReading reading = new SensorReading();
            reading.Timestamp = timestamp;
            reading.rawValue = value.ToString();
            //look up the sensor name 

            reading.SensorId = sensorId;

            readingDAO.Save(reading);

        }


    }
}
