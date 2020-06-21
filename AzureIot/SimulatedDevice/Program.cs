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
            await SendMessagesToCloudAsync();
            await ReceiveCloudMessages();
            Console.ReadLine();
        }

        private static async Task ReceiveCloudMessages()
        {
            return;
        }

        private static async Task SendMessagesToCloudAsync()
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
