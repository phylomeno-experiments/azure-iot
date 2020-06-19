using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace SimulatedDevice
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("IoT Hub Simulated Device");

            if (args.Length == 0)
            {
                Console.WriteLine("Please pass connectionString as first command line argument");
                return;
            }

            SendMessagesToHubAsync(args[0]);
            Console.ReadLine();
        }

        private static async void SendMessagesToHubAsync(string connectionString)
        {
            Console.WriteLine(connectionString);
            var deviceClient = DeviceClient.CreateFromConnectionString(connectionString);
            var rand = new Random();
            while (true)
            {
                var messageJson = JsonConvert.SerializeObject(new
                {
                    value = 45 + rand.NextDouble() * 10
                });

                var message = new Message(Encoding.ASCII.GetBytes(messageJson));
                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} - Sending message {1}", DateTime.Now, messageJson);

                await Task.Delay(1000);
            }
        }
    }
}
