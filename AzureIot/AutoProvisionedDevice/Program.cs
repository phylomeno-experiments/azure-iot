using System.Security.Cryptography.X509Certificates;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;

namespace AutoProvisionedDevice
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            X509Certificate2 certificate;
            using (var security = new SecurityProviderX509Certificate(certificate))
            using (var transport = new ProvisioningTransportHandlerMqtt())
            {
                var provisioningClient = ProvisioningDeviceClient.Create("global.azure-devices-provisioning.net",
                    "idscope", security, transport);
            }
        }
    }
}