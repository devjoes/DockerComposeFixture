using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DockerComposeFixture.Tests
{
    public class IntegrationTests : IClassFixture<DockerFixture>, IDisposable
    {
        private readonly string dockerComposeFile;

        private const string DockerCompose =
            "version: '3.4'\n" +
            "services:\n" +
            "  echo_server:\n" +
            "   image: \"hashicorp/http-echo\"\n" +
            "   ports:\n" +
            "    - 12871:8080\n" +
            "   command: -listen=:8080 -text=\"hello world\"";

        public IntegrationTests(DockerFixture dockerFixture)
        {
            this.dockerComposeFile = Path.GetTempFileName();
            File.WriteAllText(this.dockerComposeFile, DockerCompose);

            DockerFixture.Kill("echo_server").Wait();
            dockerFixture.InitOnce(() => new DockerFixtureOptions
            {
                DockerComposeFiles = new[] { this.dockerComposeFile },
                CustomUpTest = output => output.Any(l => l.Contains("Server is listening"))
            });
        }

        [Fact]
        public async Task EchoServer_SaysHello_WhenCalled()
        {
            var client = new HttpClient();
            var response = await client.GetStringAsync("http://localhost:12871");
            Assert.Contains("hello world", response);
        }

        public void Dispose()
        {
            File.Delete(this.dockerComposeFile);
        }
    }
}
