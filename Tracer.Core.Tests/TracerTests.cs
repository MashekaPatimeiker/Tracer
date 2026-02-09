using System;
using System.Threading;
using System.Linq;
using Xunit;

namespace Tracer.Core.Tests
{
    public class TracerTests
    {
        private readonly Tracer _tracer;

        public TracerTests()
        {
            _tracer = new Tracer();
        }

        [Fact]
        public void DoesNotThrow()
        {
            var exceptionCount = 0;
            var threads = new Thread[10];

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(() =>
                {
                    try
                    {
                        _tracer.StartTrace();
                        Thread.Sleep(5);
                        _tracer.StopTrace();
                        
                        _tracer.StartTrace();
                        Thread.Sleep(5);
                        _tracer.StopTrace();
                    }
                    catch
                    {
                        Interlocked.Increment(ref exceptionCount);
                    }
                });
                threads[i].Start();
            }

            foreach (var thread in threads) thread.Join();

            Assert.Equal(0, exceptionCount);
            
            var result = _tracer.GetTraceResult();
            Assert.All(result.Threads, thread => 
            {
                Assert.Equal(2, thread.Methods.Count);
            });
        }

        private void WorkerMethod()
        {
            _tracer.StartTrace();
            Thread.Sleep(50);
            _tracer.StopTrace();
        }

        private void OuterMethod(string threadName)
        {
            _tracer.StartTrace();
            Thread.Sleep(20);
            InnerMethod();
            _tracer.StopTrace();
        }

        private void InnerMethod()
        {
            _tracer.StartTrace();
            Thread.Sleep(15);
            _tracer.StopTrace();
        }
    }
}