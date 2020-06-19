using System;
using System.Threading;
using System.Threading.Tasks;

namespace EchoBackend
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("IoT Hub Echo Backend");

            if (args.Length == 0)
            {
                Console.WriteLine("Please pass eventHubEndpoint as first command line argument");
                return;
            }
            
            var cancellationSource = new CancellationTokenSource();

            var cancellationToken = CreateExitHandlerToken(cancellationSource);
            await ReceiveMessagesAsync(cancellationToken);
        }

        private static async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
        {
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
