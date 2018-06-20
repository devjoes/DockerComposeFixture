using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DockerComposeFixture.Compose
{
    public class ProcessRunner : IObservable<string>
    {
        private readonly List<IObserver<string>> observers;
        private readonly ProcessStartInfo startInfo;

        public ProcessRunner(ProcessStartInfo processStartInfo)
        {
            this.observers = new List<IObserver<string>>();
            this.startInfo = processStartInfo;
        }

        public async Task Execute()
        {
            var process = new Process { StartInfo = this.startInfo };
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;

            var tProcess = new Task(() => process.Start());
            tProcess.Start();
            await Task.Delay(10);
            var tOutput = new Task(() => this.MonitorStream(process, process.StandardOutput));
            var tErr = new Task(() => this.MonitorStream(process, process.StandardError));

            tOutput.Start();
            tErr.Start();

            await Task.WhenAll(new[] { tOutput, tErr, tProcess });
            this.observers.ForEach(o => o.OnCompleted());
        }

        private void MonitorStream(Process process, StreamReader reader)
        {
            do
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    this.observers.ForEach(o => o.OnNext(line));
                }
            } while (!process.HasExited);
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            if (!this.observers.Contains(observer))
            {
                this.observers.Add(observer);
            }

            return new Unsubscriber(this.observers, observer);
        }

        private class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<string>> observers;
            private readonly IObserver<string> observer;

            public Unsubscriber(List<IObserver<string>> observers, IObserver<string> observer)
            {
                this.observers = observers;
                this.observer = observer;
            }

            public void Dispose()
            {
                if (this.observer != null && this.observers.Contains(this.observer))
                    this.observers.Remove(this.observer);
            }
        }

    }
}