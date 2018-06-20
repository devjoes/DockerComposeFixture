using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace DockerComposeFixture.Logging
{
    public class Logger : ILogger
    {
        private readonly string logfileName;

        public Logger(string logfileName)
        {
            this.ConsoleOutput = new List<string>();
            if (logfileName != null)
            {
                if (File.Exists(logfileName))
                {
                    File.Delete(logfileName);
                }
            }
            this.logfileName = logfileName;
        }
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
            throw error;
        }


        public void OnNext(string value)
        {
            this.Log(value);
        }

        public void Log(string msg)
        {
            Debug.WriteLine(msg);
            Console.WriteLine(msg);
            this.ConsoleOutput.Add(msg);
            if (!string.IsNullOrEmpty(this.logfileName))
            {
                using (var stream = new FileStream(this.logfileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine(msg);
                    writer.Flush();
                    writer.Close();
                }
            }

        }

        public List<string> ConsoleOutput { get; }
    }
}
