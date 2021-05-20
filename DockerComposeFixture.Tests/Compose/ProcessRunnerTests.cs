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
            var psi = new ProcessStartInfo("echo", "\"test1\ntest2\ntest3\"");
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                psi = new ProcessStartInfo("cmd.exe", "/C \"echo test1& echo test2& echo test3\"");
            }
            var runner = new ProcessRunner(psi);
            runner.Subscribe(logger.Object);
            runner.Execute();
            logger.Verify(l => l.OnNext("test1"), Times.Once);
            logger.Verify(l => l.OnNext("test2"), Times.Once);
            logger.Verify(l => l.OnNext("test3"), Times.Once);
            logger.Verify(l => l.OnNext(It.IsAny<string>()), Times.Exactly(3));
            logger.Verify(l => l.OnCompleted(), Times.Once);
        }
    }
}
