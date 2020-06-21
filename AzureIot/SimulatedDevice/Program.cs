using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace SimulatedDevice
{
    public class Program
    {
        private static DeviceClient _deviceClient;

        public static async Task Main(string[] args)
        {
            Console.WriteLine("IoT Hub Simulated Device");

            if (args.Length == 0)
            {
                Console.WriteLine("Please pass connectionString as first command line argument");
                return;
            }

            ConnectDeviceClient(args[0]);
            SendMessagesToCloudAsync();
            ReceiveCloudMessagesAsync();
            Console.ReadLine();
        }

        private static async void ReceiveCloudMessagesAsync()
        {
            Console.WriteLine("Receiving cloud messages");
            while (true)
            {
                var receivedMessage = await _deviceClient.ReceiveAsync();
                if (receivedMessage == null) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Received message: {0}", Encoding.ASCII.GetString(receivedMessage.GetBytes()));
                Console.ResetColor();

                await _deviceClient.CompleteAsync(receivedMessage);
            }
        }

        private static async void SendMessagesToCloudAsync()
        {
            var rand = new Random();
            while (true)
            {
                var messageJson = JsonConvert.SerializeObject(new
                {
                    value = 45 + rand.NextDouble() * 10
                });

                var message = new Message(Encoding.ASCII.GetBytes(messageJson));
                await _deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} - Sending message {1}", DateTime.Now, messageJson);

                await Task.Delay(1000);
            }
        }

        private static void ConnectDeviceClient(string connectionString)
        {
            Console.WriteLine(connectionString);
            _deviceClient = DeviceClient.CreateFromConnectionString(connectionString);
        }
    }
}
