using System;
using System.Collections.Generic;
using System.Text;
using DockerComposeFixture.Logging;

namespace DockerComposeFixture.Exceptions
{
    public class DockerComposeException:Exception
    {
        public DockerComposeException(ILogger logger):base($"docker-compose failed - see {nameof(DockerComposeOutput)} property")
        {
            this.DockerComposeOutput = logger.ConsoleOutput.ToArray();
        }

        public string[] DockerComposeOutput { get; set; }
    }
}
