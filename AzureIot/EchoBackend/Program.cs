using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs.Consumer;
using Microsoft.Azure.Devices;

namespace EchoBackend
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("IoT Hub Echo Backend");

            if (args.Length < 3)
            {
                Console.WriteLine("Please pass eventHubCompatibleEndpoint, eventHubName and serviceClientConnectionString as command line arguments");
                return;
            }
            
            var cancellationSource = new CancellationTokenSource();

            var cancellationToken = CreateExitHandlerToken(cancellationSource);
            await ReceiveMessagesAsync(cancellationToken, args[0], args[1], args[2]);
        }

        private static async Task ReceiveMessagesAsync(CancellationToken cancellationToken, string eventHubCompatibleConnectionString, string eventHubName, string serviceClientConnectionString)
        {
            await using var client = new EventHubConsumerClient(EventHubConsumerClient.DefaultConsumerGroupName, eventHubCompatibleConnectionString, eventHubName);

            var serviceClient = ServiceClient.CreateFromConnectionString(serviceClientConnectionString);
            
            try
            {
                int messagesReceived = 0;
                await foreach (var partitionEvent in client.ReadEventsAsync(cancellationToken))
                {
                    Console.WriteLine("Message received on partition {0}", partitionEvent.Partition.PartitionId);
                    var data = Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray());
                    Console.WriteLine("\t{0}", data);

                    messagesReceived++;
                    if (messagesReceived % 50 == 0)
                    {
                        var devicedId = partitionEvent.Data.SystemProperties["iothub-connection-device-id"].ToString();
                        var message = new Message(Encoding.ASCII.GetBytes("My first Cloud-to-Device message"));
                        await serviceClient.SendAsync(devicedId, message);
                    }
                }
            }
            catch (TaskCanceledException)
            {

            }
        }

        private static CancellationToken CreateExitHandlerToken(CancellationTokenSource cancellationSource)
        {
            void CancelKeyPressHandler(object sender, ConsoleCancelEventArgs eventArgs)
            {
                eventArgs.Cancel = true;
                cancellationSource.Cancel();
                Console.WriteLine("Exiting...");

                Console.CancelKeyPress -= CancelKeyPressHandler;
            }

            Console.CancelKeyPress += CancelKeyPressHandler;

            var cancellationToken = cancellationSource.Token;
            return cancellationToken;
        }
    }
}
