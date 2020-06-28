using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs.Consumer;
using Microsoft.Azure.Devices;

namespace EchoBackend
{
    internal class Program
    {
        private static ServiceClient _serviceClient;
        private static EventHubConsumerClient _eventHubConsumerClient;
        private static RegistryManager _registryManager;
        private static CancellationToken _cancellationToken;

        private static async Task Main(string[] args)
        {
            Console.WriteLine("IoT Hub Echo Backend");

            if (args.Length < 3)
            {
                Console.WriteLine(
                    "Please pass eventHubCompatibleEndpoint, eventHubName and serviceClientConnectionString as command line arguments");
                return;
            }

            SetupCancellationToken();
            SetupClients(args[0], args[1], args[2]);
            await HandleFileUploads();
            await TagAndQueryDevices();
            await ReadReportedProperties();
            await ListenForMessages();
        }

        private static async Task TagAndQueryDevices()
        {
            var twin = await _registryManager.GetTwinAsync("device1");

            var tagsPatch = @"{
                tags: {
                    provisioning: {
                        method: 'auto'
                    }
                }
            }";

            await _registryManager.UpdateTwinAsync(twin.DeviceId, tagsPatch, twin.ETag);

            var twinQuery = _registryManager.CreateQuery(
                "SELECT * FROM devices WHERE tags.provisioning.method = 'auto'", 100);

            var autoProvisionedTwin = await twinQuery.GetNextAsTwinAsync();
            Console.WriteLine($"{autoProvisionedTwin.First().DeviceId} was auto-provisioned");
        }

        private static async Task ReadReportedProperties()
        {
            var twin2 = await _registryManager.GetTwinAsync("phylomeno-pi");
            foreach (var property in twin2.Properties.Reported)
            {
                Console.WriteLine($"Reported property: {property}");
            }
        }

        private static void SetupCancellationToken()
        {
            var cancellationSource = new CancellationTokenSource();

            void CancelKeyPressHandler(object sender, ConsoleCancelEventArgs eventArgs)
            {
                eventArgs.Cancel = true;
                cancellationSource.Cancel();
                Console.WriteLine("Exiting...");

                Console.CancelKeyPress -= CancelKeyPressHandler;
            }

            Console.CancelKeyPress += CancelKeyPressHandler;

            _cancellationToken = cancellationSource.Token;
        }

        private static void SetupClients(string eventHubCompatibleConnectionString, string eventHubName,
            string serviceClientConnectionString)
        {
            _eventHubConsumerClient = new EventHubConsumerClient(EventHubConsumerClient.DefaultConsumerGroupName,
                eventHubCompatibleConnectionString, eventHubName);

            _serviceClient = ServiceClient.CreateFromConnectionString(serviceClientConnectionString);

            _registryManager = RegistryManager.CreateFromConnectionString(serviceClientConnectionString);
        }

        private static async Task HandleFileUploads()
        {
            await Task.Run(() => ListenForFileUploads(_serviceClient), _cancellationToken);
        }

        private static async Task ListenForMessages()
        {
            try
            {
                var messagesReceived = 0;
                await foreach (var partitionEvent in _eventHubConsumerClient.ReadEventsAsync(_cancellationToken))
                {
                    Console.WriteLine("Message received on partition {0}", partitionEvent.Partition.PartitionId);
                    var data = Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray());
                    Console.WriteLine("\t{0}", data);

                    messagesReceived++;
                    var deviceId = partitionEvent.Data.SystemProperties["iothub-connection-device-id"].ToString();
                    if (messagesReceived % 50 == 0)
                    {
                        await SendMessageToDevice(deviceId);
                    }

                    else if (messagesReceived % 10 == 0)
                    {
                        await InvokeDeviceMethod(deviceId);
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        private static async Task SendMessageToDevice(string deviceId)
        {
            var message = new Message(Encoding.ASCII.GetBytes("My first Cloud-to-Device message"));
            await _serviceClient.SendAsync(deviceId, message);
            Console.WriteLine("Sent message to device");
        }

        private static async Task InvokeDeviceMethod(string deviceId)
        {
            var method = new CloudToDeviceMethod("my-method") {ResponseTimeout = TimeSpan.FromSeconds(30)};
            await _serviceClient.InvokeDeviceMethodAsync(deviceId, method, _cancellationToken);
            Console.WriteLine("Invoked method on device");
            var twin = await _registryManager.GetTwinAsync(deviceId, _cancellationToken);
            Console.WriteLine(twin.Properties.Reported.ToJson());
        }

        private static async void ListenForFileUploads(ServiceClient serviceClient)
        {
            var notificationReceiver = serviceClient.GetFileNotificationReceiver();
            Console.WriteLine("Listening for file upload notifications");
            while (true)
            {
                var fileUploadNotification = await notificationReceiver.ReceiveAsync();
                if (fileUploadNotification == null) continue;

                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine("Received file upload notification: {0}",
                    string.Join(" ", fileUploadNotification.BlobName));
                Console.ResetColor();

                await notificationReceiver.CompleteAsync(fileUploadNotification);
            }
        }
    }
}