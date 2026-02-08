using System.Collections.Generic;

namespace Tracer.Core
{
    public class TraceResult
    {
        public IReadOnlyList<ThreadTraceResult> Threads { get; }

        internal TraceResult(List<ThreadTraceResult> threads)
        {
            Threads = threads.AsReadOnly();
        }
    }

    public class ThreadTraceResult
    {
        public int Id { get; }
        public long Time { get; }
        public IReadOnlyList<MethodTraceResult> Methods { get; }

        internal ThreadTraceResult(int id, long time, List<MethodTraceResult> methods)
        {
            Id = id;
            Time = time;
            Methods = methods.AsReadOnly();
        }
    }

    public class MethodTraceResult
    {
        public string Name { get; }
        public string Class { get; }
        public long Time { get; }
        public IReadOnlyList<MethodTraceResult> Methods { get; }

        internal MethodTraceResult(string name, string className, long time, List<MethodTraceResult> methods)
        {
            Name = name;
            Class = className;
            Time = time;
            Methods = methods.AsReadOnly();
        }
    }
}