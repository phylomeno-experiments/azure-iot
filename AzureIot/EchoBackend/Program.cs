using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs.Consumer;

namespace EchoBackend
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("IoT Hub Echo Backend");

            if (args.Length < 2)
            {
                Console.WriteLine("Please pass eventHubEndpoint and eventHubName as command line arguments");
                return;
            }
            
            var cancellationSource = new CancellationTokenSource();

            var cancellationToken = CreateExitHandlerToken(cancellationSource);
            await ReceiveMessagesAsync(cancellationToken, args[0], args[1]);
        }

        private static async Task ReceiveMessagesAsync(CancellationToken cancellationToken, string connectionString, string eventHubName)
        {
            await using var client = new EventHubConsumerClient(EventHubConsumerClient.DefaultConsumerGroupName, connectionString, eventHubName);

            try
            {
                await foreach (var partitionEvent in client.ReadEventsAsync(cancellationToken))
                {
                    Console.WriteLine("Message received on partition {0}", partitionEvent.Partition.PartitionId);
                    var data = Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray());
                    Console.WriteLine("\t{0}", data);
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
