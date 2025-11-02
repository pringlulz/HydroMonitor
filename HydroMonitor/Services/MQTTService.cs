using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;

namespace HydroMonitor.Services
{
    

    public class MQTTService
    {
        private static IMqttClient? _client;

        public MQTTService()
        {
            OpenAndSubscribe();
        }

        public static async Task OpenAndSubscribe()
        {
            System.Diagnostics.Debug.WriteLine("Starting subscribe...");
            var factory = new MqttClientFactory();

            var mqttClient = factory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder().WithTcpServer("192.168.0.9").Build();


            await mqttClient.ConnectAsync(options, CancellationToken.None);

            var topicFilter = factory.CreateTopicFilterBuilder().WithTopic("#").WithAtLeastOnceQoS();

            var subscribeOptions = factory.CreateSubscribeOptionsBuilder().WithTopicFilter(topicFilter).Build();

            var response = await mqttClient.SubscribeAsync(subscribeOptions, CancellationToken.None);

            System.Diagnostics.Debug.WriteLine("MQTT Client subscribed to topic.");

            System.Diagnostics.Debug.WriteLine(response.Items.Count);
            System.Diagnostics.Debug.WriteLine(response.Items.ElementAt(0).ResultCode );
            System.Diagnostics.Debug.WriteLine(response.Items.ElementAt(0).TopicFilter);

            //this is the response handler
            mqttClient.ApplicationMessageReceivedAsync += m =>
            {
                System.Diagnostics.Debug.WriteLine($"Received Message: {m.ApplicationMessage.ConvertPayloadToString()}");
                return Task.CompletedTask;
            };
            _client = mqttClient;
        }


    }
}
