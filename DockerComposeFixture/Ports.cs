using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace DockerComposeFixture
{
    public static class Ports
    {
        // check if network port is open
        public static ushort[] DetermineUtilizedPorts(ushort[] ports)
        {
            return ports.Where(p => !IsPortAvailable(p)).ToArray();
        }

        private static bool IsPortAvailable(UInt16 port)
        {
            var isAvailable = true;
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

            foreach (var endpoint in tcpConnInfoArray) {
                if (endpoint.Port == port) {
                    isAvailable = false;
                    break;
                }
            }

            return isAvailable;
        }
    }
}