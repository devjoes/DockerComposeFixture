using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DockerComposeFixture.Logging;
using DockerComposeFixture.Tests.Utils;
using FluentAssertions;
using Xunit;

namespace DockerComposeFixture.Tests.Logging
{
    public class LoggerTests
    {
        [Fact]
        public async Task OnNext_LogsItemsToFile_WhenCalled()
        {
            var tmpFile = Path.GetTempFileName();
            
            int GetFileLineCount(string file)
            {
                using (var fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Write))
                using (var reader = new StreamReader(fs))
                {
                    return reader.ReadToEnd()
                        .Split(Environment.NewLine)
                        .Count(l => l.Length > 0);
                }
            }

            var loggers = new ILogger[]{ new ListLogger(), new FileLogger(tmpFile), new ConsoleLogger() };
            var counter = new ObservableCounter();
            foreach (var logger in loggers)
            {
                counter.Subscribe(logger);
            }
            
            var task = new Task(() => counter.Count(delay: 10));
            task.Start();
            await task;
            
            var fileLineCount = GetFileLineCount(tmpFile);
            fileLineCount.Should().Be(10);
            var lines = File.ReadAllLines(tmpFile);
            lines.Should().BeEquivalentTo("1,2,3,4,5,6,7,8,9,10".Split(","));
            File.Delete(tmpFile);
        }
    }
}
