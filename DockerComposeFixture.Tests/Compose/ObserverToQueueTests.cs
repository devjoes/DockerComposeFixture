using DockerComposeFixture.Compose;
using DockerComposeFixture.Tests.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DockerComposeFixture.Tests.Compose
{
    public class ObserverToQueueTests
    {
        [Fact]
        public async Task OnNext_EnqueuesItems_WhenCalled()
        {
            var observerToQueue = new ObserverToQueue<string>();
            var counter = new ObservableCounter();
            counter.Subscribe(observerToQueue);
            var task = new Task(() => counter.Count());
            Assert.Empty(observerToQueue.Queue);
            
            task.Start();
            await task;
            
            Assert.Equal(10, observerToQueue.Queue.Count);
            Assert.Equal("1,2,3,4,5,6,7,8,9,10".Split(","),
                observerToQueue.Queue.ToArray());
        }
    }
}
