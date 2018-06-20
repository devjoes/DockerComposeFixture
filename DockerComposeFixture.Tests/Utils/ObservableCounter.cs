using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DockerComposeFixture.Tests.Utils
{
    public class ObservableCounter:IObservable<string>
    {
        private readonly List<IObserver<string>> observalbes = new List<IObserver<string>>();
        public IDisposable Subscribe(IObserver<string> observer)
        {
            this.observalbes.Add(observer);
            return null;
        }

        public void Count(int min = 1, int max = 10, int delay = 10)
        {
            for (int i = min; i <= max; i++)
            {
                this.observalbes.ForEach(o => o.OnNext(i.ToString()));
                Thread.Sleep(delay);
            }
            this.observalbes.ForEach(o => o.OnCompleted());
        }
    }
}
