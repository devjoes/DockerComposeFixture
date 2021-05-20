using System;

namespace DockerComposeFixture.Logging
{
    public interface ILogger : IObserver<string>
    {
        void Log(string msg);
    }
}