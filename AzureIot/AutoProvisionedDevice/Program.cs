using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;

namespace AutoProvisionedDevice
{
    internal class Program
    {
        private static async void Main(string[] args)
        {
            X509Certificate2 certificate;
            DeviceAuthenticationWithX509Certificate auth;
            string iotHub;
            using (var security = new SecurityProviderX509Certificate(certificate))
            {
                using (var transport = new ProvisioningTransportHandlerMqtt())
                {
                    var provisioningClient = ProvisioningDeviceClient.Create("global.azure-devices-provisioning.net",
                        "idscope", security, transport);

                    var provisioningResult = await provisioningClient.RegisterAsync();

                    Console.WriteLine($"Provisioning done - Assigned Hub: {provisioningResult.AssignedHub} - DeviceID {provisioningResult.DeviceId}");

                    auth = new DeviceAuthenticationWithX509Certificate(provisioningResult.DeviceId, security.GetAuthenticationCertificate());
                    iotHub = provisioningResult.AssignedHub;
                }
            }

            using (var deviceClient = DeviceClient.Create(iotHub, auth, TransportType.Mqtt))
            {
                await deviceClient.OpenAsync();
                await deviceClient.SendEventAsync(
                    new Message(Encoding.UTF8.GetBytes("Auto provisioned device was here")));
                await deviceClient.CloseAsync();

                Console.WriteLine("Sent message");
            }
        }
    }
}