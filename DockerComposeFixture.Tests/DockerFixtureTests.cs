using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DockerComposeFixture.Logging;
using DockerComposeFixture.Compose;
using DockerComposeFixture.Exceptions;
using Moq;
using Xunit;

namespace DockerComposeFixture.Tests
{
    public class DockerFixtureTests
    {
        private const int NumberOfMsInOneSec = 10; // makes testing faster
        private const int ComposeUpRunDurationMs = 5000;

        [Fact]
        public void Init_StopsDocker_IfAlreadyRunning()
        {
            var compose = new Mock<IDockerCompose>();
            compose.Setup(c => c.PauseMs).Returns(NumberOfMsInOneSec);
            compose.Setup(c => c.Ps())
                .Returns(new[] { "--------", " Up ", " Up " });
            compose.Setup(c => c.Up()).Returns(Task.Delay(ComposeUpRunDurationMs));

            new DockerFixture(null)
                .Init(new[] { Path.GetTempFileName() }, "up", "down", 120, null, compose.Object);
            compose.Verify(c => c.Init(It.IsAny<string>(), "up", "down"), Times.Once);
            compose.Verify(c => c.Down(), Times.Once);

        }

        [Fact]
        public void Init_InitialisesDocker_WhenCalled()
        {
            var compose = new Mock<IDockerCompose>();
            compose.Setup(c => c.PauseMs).Returns(NumberOfMsInOneSec);
            compose.SetupSequence(c => c.Ps())
                .Returns(new[] { "--------" })
                .Returns(new[] { "--------", " Up ", " Up " });
            compose.Setup(c => c.Up()).Returns(Task.Delay(ComposeUpRunDurationMs));

            var tmp = Path.GetTempFileName();
            new DockerFixture(null)
                .Init(new[] { tmp }, "up", "down", 120, null, compose.Object);
            compose.Verify(c => c.Init($"-f \"{tmp}\"", "up", "down"), Times.Once);
            compose.Verify(c => c.Up(), Times.Once);
        }

        [Fact]
        public void InitOnce_InitialisesDockerOnce_WhenCalledTwice()
        {
            var compose = new Mock<IDockerCompose>();
            compose.Setup(c => c.PauseMs).Returns(NumberOfMsInOneSec);
            compose.Setup(c => c.Up()).Returns(Task.Delay(ComposeUpRunDurationMs));
            compose.SetupSequence(c => c.Ps())
                .Returns(new[] { "--------" })
                .Returns(new[] { "--------", " Up ", " Up " });

            var tmp = Path.GetTempFileName();
            var fixture = new DockerFixture(null);

            fixture.InitOnce(() => new DockerFixtureOptions { DockerComposeFiles = new[] { tmp } }, compose.Object);
            fixture.InitOnce(() => new DockerFixtureOptions { DockerComposeFiles = new[] { tmp } }, compose.Object);
            compose.Verify(c => c.Init(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            compose.Verify(c => c.Up(), Times.Once);
        }

        [Fact]
        public void Init_Waits_UntilUpTestIsTrue()
        {
            var compose = new Mock<IDockerCompose>();
            compose.Setup(c => c.PauseMs).Returns(NumberOfMsInOneSec);
            compose.Setup(c => c.Up()).Returns(Task.Delay(5000));
            compose.Setup(c => c.Ps())
                .Returns(new[] { "--------", " Up ", " Up " });
            const string successText = "Everything is up";

            var logger = new ListLogger();
            var task = new Task(() =>
                 new DockerFixture(null)
                    .Init(new[] { Path.GetTempFileName() }, "up", "down", 120,
                        outputLinesFromUp => outputLinesFromUp.Contains(successText),
                        compose.Object, new []{ logger}));
            task.Start();
            Thread.Sleep(100);
            logger.OnNext("foo");
            logger.OnNext(successText);
            task.Wait();

            compose.Verify(c => c.Init(It.IsAny<string>(), "up", "down"), Times.Once);
            compose.Verify(c => c.Up(), Times.Once);
        }

        [Fact]
        public void Init_Throws_IfTestIsNeverTrue()
        {
            var compose = new Mock<IDockerCompose>();
            compose.Setup(c => c.PauseMs).Returns(NumberOfMsInOneSec);
            compose.Setup(c => c.Ps())
                .Returns(new[] { "--------", " Up ", " Up " });
            const string successText = "Everything is up";
            var logger = new ListLogger();
            compose.SetupGet(c => c.Logger).Returns(new []{ logger});

            Assert.Throws<AggregateException>(() =>
            {
                var task = new Task(() =>
                    new DockerFixture(null)
                        .Init(new[] { Path.GetTempFileName() }, "up", "down", 120,
                            outputLinesFromUp => outputLinesFromUp.Contains(successText),
                            compose.Object));
                task.Start();
                logger.OnNext("foo");
                logger.OnNext("bar");
                Thread.Sleep(100);
                logger.OnNext("foo");
                logger.OnNext("bar");
                task.Wait();
            });
        }

        [Fact]
        public void Init_MonitorsServices_WhenTheyStartSlowly()
        {
            Stopwatch stopwatch = new Stopwatch();
            var compose = new Mock<IDockerCompose>();
            compose.Setup(c => c.PauseMs).Returns(100);
            compose.Setup(c => c.Up())
                .Returns(Task.Delay(ComposeUpRunDurationMs))
                .Callback(() => stopwatch.Start());
            compose.Setup(c => c.Ps()).Returns(() =>
            {
                if (!stopwatch.IsRunning)
                {
                    return new[] { "--------" };
                }
                string firstServiceStatus = stopwatch.ElapsedMilliseconds < 1000 ? " Starting " : " Up ";
                string secondServiceStatus = stopwatch.ElapsedMilliseconds < 3000 ? " Starting " : " Up ";
                return new[]
                {
                    "blah",
                    "--------",
                    $"14f227387e7c\ttestservice1\t\"dotnet TestService...\"\t20 seconds ago\t{firstServiceStatus}\t0.0.0.0:32769->80/tcp\ttestservice_testservice_1",
                    $"14f227387e7d\ttestservice2\t\"dotnet TestService...\"\t20 seconds ago\t{secondServiceStatus}\t0.0.0.0:32769->89/tcp\ttestservice_testservice_2"
                };
            });

            new DockerFixture(null).Init(new[] { Path.GetTempFileName() }, "up", "down", 120, null, compose.Object);

            compose.Verify(c => c.Up(), Times.Once);
            compose.Verify(c => c.Ps(), Times.AtLeast(5));
        }


        [Fact]
        public void Init_Throws_WhenServicesFailToStart()
        {
            var compose = new Mock<IDockerCompose>();
            compose.Setup(c => c.PauseMs).Returns(NumberOfMsInOneSec);
            bool firstTime = true;
            compose.Setup(c => c.Ps())
                .Returns(() =>
                {
                    var data = firstTime ? new[] { "--------" } : new[] { "--------", " Down ", " Down " };
                    firstTime = false;
                    return data;
                });

            Assert.Throws<DockerComposeException>(() =>
                new DockerFixture(null).Init(new[] { Path.GetTempFileName() }, "up", "down", 120, null, compose.Object));
        }


        [Fact]
        public void Init_Throws_DockerComposeExitsPrematurely()
        {
            var compose = new Mock<IDockerCompose>();
            compose.Setup(c => c.PauseMs).Returns(NumberOfMsInOneSec);
            compose.Setup(c => c.Up()).Returns(Task.CompletedTask);
            
            Assert.Throws<DockerComposeException>(() =>
                new DockerFixture(null).Init(new[] { Path.GetTempFileName() }, "up", "down", 120, null, compose.Object));
            compose.Verify(c => c.Ps(), Times.Once);
        }

        [Fact]
        public void Init_Throws_WhenYmlFileDoesntExist()
        {
            var compose = new Mock<IDockerCompose>();
            compose.Setup(c => c.PauseMs).Returns(NumberOfMsInOneSec);
            compose.Setup(c => c.Ps())
                .Returns(new[] { "--------", " Up ", " Up " });

            string fileDoesntExist = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Assert.Throws<ArgumentException>(() =>
                new DockerFixture(null).Init(new[] { fileDoesntExist }, "up", "down", 120, null, compose.Object));
        }

        [Fact]
        public void Init_Throws_WhenDockerComposeFilesAreMissing()
        {
            var compose = new Mock<IDockerCompose>();

            Assert.Throws<ArgumentException>(() =>
                new DockerFixture(null).Init(
                    () => new DockerFixtureOptions { DockerComposeFiles = new string[0] },
                    compose.Object));
            Assert.Throws<ArgumentException>(() =>
                new DockerFixture(null).Init(
                    () => new DockerFixtureOptions { DockerComposeFiles = null },
                    compose.Object));
        }

        [Fact]
        public void Init_Throws_WhenStartupTimeoutSecsIsLessThanOne()
        {
            var compose = new Mock<IDockerCompose>();
            
            Assert.Throws<ArgumentException>(() =>
                new DockerFixture(null).Init(
                    () => new DockerFixtureOptions { DockerComposeFiles = new[] { "docker-compose.yml" }, StartupTimeoutSecs = 0 },
                    compose.Object));
            Assert.Throws<ArgumentException>(() =>
                new DockerFixture(null).Init(
                    () => new DockerFixtureOptions { DockerComposeFiles = new[] { "docker-compose.yml" }, StartupTimeoutSecs = -1 },
                    compose.Object));
        }



        [Fact]
        public void Dispose_CallsDown_WhenRun()
        {
            var compose = new Mock<IDockerCompose>();
            compose.Setup(c => c.PauseMs).Returns(NumberOfMsInOneSec);
            compose.Setup(c => c.Up()).Returns(Task.Delay(ComposeUpRunDurationMs));
            compose.SetupSequence(c => c.Ps())
                .Returns(new[] { "--------" })
                .Returns(new[] { "--------", " Up ", " Up " });

            var fixture = new DockerFixture(null);
            fixture.Init(new[] { Path.GetTempFileName() }, "up", "down", 120, null, compose.Object);
            fixture.Dispose();

            compose.Verify(c => c.Down(), Times.Once);
        }

    }
}
