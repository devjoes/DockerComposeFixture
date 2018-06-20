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

namespace DockerComposeFixture.Tests.Logging
{
    public class LoggerTests
    {
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

            var logger = new Logger(tmpFile);
            var counter = new ObservableCounter();
            counter.Subscribe(logger);
            var task = new Task(() => counter.Count());
            
            task.Start();
            Thread.Sleep(30);
            Assert.True(GetFileLineCount(tmpFile) > 0);
            Assert.True(GetFileLineCount(tmpFile) < 10);
            task.Wait();
            Assert.Equal(10, GetFileLineCount(tmpFile));
            Assert.Equal("1,2,3,4,5,6,7,8,9,10".Split(","),
                File.ReadAllLines(tmpFile));
            File.Delete(tmpFile);
        }
    }
}
