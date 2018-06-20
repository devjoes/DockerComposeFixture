using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DockerComposeFixture.Compose;
using DockerComposeFixture.Logging;
using Moq;
using Xunit;

namespace DockerComposeFixture.Tests.Compose
{
    public class ProcessRunnerTests
    {
        [Fact]
        public void Execute_ReturnsOutput_WhenCalled()
        {
            var  logger = new Mock<ILogger>();
            var runner = new ProcessRunner(new ProcessStartInfo("echo", "\"test1\ntest2\ntest3\""));
            runner.Subscribe(logger.Object);
            runner.Execute().Wait();
            logger.Verify(l => l.OnNext(It.IsAny<string>()));
        }
    }
}
