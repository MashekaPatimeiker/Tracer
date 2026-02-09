using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tracer.Core;
using Tracer.Serialization;

namespace Tracer.Example
{
    public class Foo
    {
        private readonly ITracer _tracer;

        public Foo(ITracer tracer)
        {
            _tracer = tracer;
        }

        public void MyMethod()
        {
            _tracer.StartTrace();
            Thread.Sleep(100);
            Bar bar = new Bar(_tracer);
            bar.InnerMethod();
            Thread.Sleep(50);
            _tracer.StopTrace();
        }
    }

    public class Bar
    {
        private readonly ITracer _tracer;

        public Bar(ITracer tracer)
        {
            _tracer = tracer;
        }

        public void InnerMethod()
        {
            _tracer.StartTrace();
            Thread.Sleep(75);
            _tracer.StopTrace();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var tracer = new Tracer.Core.Tracer();

            Foo foo = new Foo(tracer);
            foo.MyMethod();

            Task task1 = Task.Run(() =>
            {
                tracer.StartTrace();
                Thread.Sleep(200);
                tracer.StopTrace();
            });

            Task task2 = Task.Run(() =>
            {
                tracer.StartTrace();
                Thread.Sleep(100);
                tracer.StopTrace();
            });

            Task.WaitAll(task1, task2);

            var traceResult = tracer.GetTraceResult();

            var serializers = SerializerLoader.LoadSerializers();
            
            foreach (var serializer in serializers)
            {
                string fileName = $"result.{serializer.Format}";
                using (var fileStream = File.Create(fileName))
                {
                    serializer.Serialize(traceResult, fileStream);
                }
                Console.WriteLine($"Results saved to {fileName}");
            }

            Console.WriteLine($"Total threads traced: {traceResult.Threads.Count}");
        }
    }
}