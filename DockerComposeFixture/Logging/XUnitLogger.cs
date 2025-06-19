using System;
using Xunit.Sdk;
using Xunit.v3;

namespace DockerComposeFixture.Logging
{
    public class XUnitLogger : ILogger
    {
        private readonly IMessageSink xlogOutput;

        public XUnitLogger(IMessageSink xlogOutput)
        {
            this.xlogOutput = xlogOutput;
        }

        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {
            this.Log(error.Message + "\n" + error.StackTrace);
            throw error;
        }

        public void OnNext(string value)
        {
            this.Log(value);
        }

        public void Log(string msg)
        {
            this.xlogOutput.OnMessage(new DiagnosticMessage(msg));
        }
    }
}