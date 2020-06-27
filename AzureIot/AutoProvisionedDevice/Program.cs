using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;

namespace AutoProvisionedDevice
{
    internal class Program
    {
        private const string CertificateFileName = "device1.pfx";
        private const string CertificateFilePassword = "password";

        private static async Task Main(string[] args)
        {
            Console.WriteLine("DPS auto provisioned device");

            if (args.Length == 0)
            {
                Console.WriteLine("Please pass ID Scope as first command line argument");
                return;
            }

            var idScope = args[0];

            var certificate = LoadCertificate();
            DeviceAuthenticationWithX509Certificate auth;
            string iotHub;

            using (var security = new SecurityProviderX509Certificate(certificate))
            {
                using (var transport = new ProvisioningTransportHandlerMqtt())
                {
                    var provisioningClient = ProvisioningDeviceClient.Create("global.azure-devices-provisioning.net",
                        idScope, security, transport);

                    var provisioningResult = await provisioningClient.RegisterAsync();

                    Console.WriteLine(
                        $"Provisioning done - Assigned Hub: {provisioningResult.AssignedHub} - DeviceID {provisioningResult.DeviceId}");

                    auth = new DeviceAuthenticationWithX509Certificate(provisioningResult.DeviceId,
                        security.GetAuthenticationCertificate());
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

        private static X509Certificate2 LoadCertificate()
        {
            var certificateCollection = new X509Certificate2Collection();
            certificateCollection.Import(CertificateFileName, CertificateFilePassword, X509KeyStorageFlags.UserKeySet);

            X509Certificate2 finalCertificate = null;
            foreach (var certificate in certificateCollection)
                if (finalCertificate == null && certificate.HasPrivateKey)
                    finalCertificate = certificate;
                else
                    certificate.Dispose();

            if (finalCertificate == null)
                throw new FileNotFoundException(
                    $"File {CertificateFileName} did not contain certificate with private key");

            return finalCertificate;
        }
    }
}