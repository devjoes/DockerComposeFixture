using System;

namespace DockerComposeFixture.Exceptions
{
    public class PortsUnavailableException : DockerComposeException
    {
        public PortsUnavailableException(string[] loggedLines, ushort[] ports) : base(loggedLines)
        {
            Ports = ports;
        }

        public ushort[] Ports { get; set; }
    }
}