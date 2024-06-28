using System;
using System.Text;
using MQTTnet;
using MQTTnet.Client;
using NATS.Client;
using Newtonsoft.Json;

namespace Filter
{
    internal class Program
    {
       
        static Dictionary<int,float> avgTemp = new Dictionary<int,float>();
        static Dictionary<int,float> avgHum = new Dictionary<int,float>();
        static Dictionary<int,float> count = new Dictionary<int,float>();
       
        public static async Task Handle_Received_Application_Message()
        {
            
            var mqttFactory = new MqttFactory();

            // Define NATS connection options
            Options opts = ConnectionFactory.GetDefaultOptions();
            opts.Url = "nats://natsumrezi:4222";  // Update the URL if your NATS server is hosted elsewhere

            // Create a connection factory
            IConnectionFactory cf = new ConnectionFactory();

            // Connect to the NATS server
            using (IConnection conn = cf.CreateConnection(opts))
            {
                // Define a subject (topic) to subscribe and publish to
                string subject = "prosek";


                using (var mqttClient = mqttFactory.CreateMqttClient())
                {
                    var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("mqttumrezi").Build();

                    mqttClient.ApplicationMessageReceivedAsync += e =>
                    {

                        var message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                        var vrednostSenzora = JsonConvert.DeserializeObject<MerenjeDTO>(message);
                        Console.WriteLine("Received application message.");
                        Console.WriteLine(message);

                        if (!avgTemp.ContainsKey(vrednostSenzora.Device))
                        {
                            avgTemp.Add(vrednostSenzora.Device, vrednostSenzora.Temperature);
                            avgHum.Add(vrednostSenzora.Device, vrednostSenzora.Humidity);
                            count.Add(vrednostSenzora.Device, 1);
                        }
                        else
                        {
                            avgTemp[vrednostSenzora.Device] += vrednostSenzora.Temperature;
                            avgHum[vrednostSenzora.Device] += vrednostSenzora.Humidity;
                            count[vrednostSenzora.Device] += 1;
                        }

                        //send analytics on every 10th message
                        if (count[vrednostSenzora.Device] == 10)
                        {
                            //DateTime now = DateTime.UtcNow;
                            //string formattedDate = now.ToString("o"); // This will give you a string in RFC3339 format

                            AnalyticsDTO analyticsDTO = new AnalyticsDTO
                            {

                                DateTime = vrednostSenzora.Date,
                                Device = vrednostSenzora.Device,
                                Battery = vrednostSenzora.Battery,
                                AverageHumidity = avgHum[vrednostSenzora.Device] / count[vrednostSenzora.Device],
                                AverageTemperature = avgTemp[vrednostSenzora.Device] / count[vrednostSenzora.Device]
                            };

                            avgTemp[vrednostSenzora.Device] = 0;
                            avgHum[vrednostSenzora.Device] = 0; 
                            count[vrednostSenzora.Device] = 0;


                            string json = JsonConvert.SerializeObject(analyticsDTO);
                            byte[] data = Encoding.UTF8.GetBytes(json);
                            // Publish a message to the subject
                            conn.Publish(subject, data);

                            // Flush the connection to ensure the message is sent
                            conn.Flush();
                            Console.WriteLine($"\nPublished on nats: {json}\n");

                            /*
                            var mqttApplicationMessage = new MqttApplicationMessageBuilder()
                                .WithTopic("analytics")
                                .WithPayload(JsonConvert.SerializeObject(analyticsDTO))
                                .Build();

                            mqttClient.PublishAsync(mqttApplicationMessage, CancellationToken.None);
                            Console.WriteLine("Analytics sent.");
                            */
                        }
                        /*

                        String poruka = "";
                        if (vrednostSenzora.Temperature < 20)
                            poruka = "Temperatura je ispod 20 stepeni\n";
                        if (vrednostSenzora.Humidity < 30)
                            poruka += "Vlaznost vazduha je ispod 30%.\n";
                        if(vrednostSenzora.Battery < 1)
                            poruka += "Baterija je prazna.";

                        if(poruka != "")
                        { 
                            EventDTO eventDTO = new EventDTO
                            {
                                Date = vrednostSenzora.Date,
                                Time = vrednostSenzora.Time,
                                Device = vrednostSenzora.Device,
                                Battery = vrednostSenzora.Battery,
                                Humidity = vrednostSenzora.Humidity,
                                Temperature = vrednostSenzora.Temperature,
                                Message = poruka
                            };
                            var mqttApplicationMessage = new MqttApplicationMessageBuilder()
                                .WithTopic("event")
                                .WithPayload(JsonConvert.SerializeObject(eventDTO))
                                .Build();

                            mqttClient.PublishAsync(mqttApplicationMessage, CancellationToken.None);
                            Console.WriteLine("Event sent.");
                        }
                        */

                        return Task.CompletedTask;
                    };

                    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                    var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                        .WithTopicFilter(
                            f =>
                            {
                                f.WithTopic("merenje");
                            })
                        .Build();

                    await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

                    Console.WriteLine("MQTT client subscribed to topic merenje.");

                    while (true)
                    {
                        await Task.Delay(TimeSpan.FromMinutes(1));
                    }



                }
            }
        }

      
        //on sigint disconnect from mqtt
        static void OnExit(object sender, ConsoleCancelEventArgs args)
        {
            var mqttFactory = new MqttFactory();
            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientDisconnectOptions = mqttFactory.CreateClientDisconnectOptionsBuilder().Build();

                mqttClient.DisconnectAsync(mqttClientDisconnectOptions, CancellationToken.None);
            }
        }
      
        static async Task Main(string[] args)
        {
            Console.WriteLine("Analytics servise");

            //await Task.Delay(10000);
            await Handle_Received_Application_Message();
            
          
        }

       
    }
}
