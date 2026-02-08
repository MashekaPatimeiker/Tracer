using System;
using System.Threading;
using Xunit;

namespace Tracer.Core.Tests
{
    public class TracerTests
    {
        [Fact]
        public void SingleMethodTrace_ShouldRecordCorrectTime()
        {
            var tracer = new Tracer();
            tracer.StartTrace();
            Thread.Sleep(100);
            tracer.StopTrace();

            var result = tracer.GetTraceResult();
            Assert.Single(result.Threads);
            Assert.Single(result.Threads[0].Methods);
            Assert.True(result.Threads[0].Methods[0].Time >= 100);
        }

        [Fact]
        public void NestedMethods_ShouldHaveCorrectHierarchy()
        {
            var tracer = new Tracer();
            tracer.StartTrace();
            tracer.StartTrace();
            Thread.Sleep(50);
            tracer.StopTrace();
            tracer.StopTrace();

            var result = tracer.GetTraceResult();
            Assert.Single(result.Threads[0].Methods);
            Assert.Single(result.Threads[0].Methods[0].Methods);
        }
    }
}