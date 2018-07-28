using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DockerComposeFixture.Logging;
using DockerComposeFixture.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace DockerComposeFixture.Tests.Logging
{
    public class LoggerTests
    {
        public LoggerTests()
        {
        }

        [Fact]
        public void OnNext_LogsItemsToFile_WhenCalled()
        {
            string tmpFile = Path.GetTempFileName();
            int GetFileLineCount(string file)
            {
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Write))
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
            
            var task = new Task(() => counter.Count());
            
            task.Start();
            Thread.Sleep(30);
            Assert.True(GetFileLineCount(tmpFile) > 0);
            Assert.True(GetFileLineCount(tmpFile) < 10);
            task.Wait();
            Assert.Equal(10, GetFileLineCount(tmpFile));
            var lines = File.ReadAllLines(tmpFile);
            Assert.Equal("1,2,3,4,5,6,7,8,9,10".Split(","), lines);
            File.Delete(tmpFile);
        }
    }
}
