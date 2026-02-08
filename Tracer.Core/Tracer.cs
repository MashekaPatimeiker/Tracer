using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Tracer.Core
{
    public class Tracer : ITracer
    {
        private class ThreadTraceInfo
        {
            public Stack<MethodTraceInfo> CallStack { get; } = new();
            public List<MethodTraceInfo> RootMethods { get; } = new();
            public long TotalTime => RootMethods.Sum(m => m.TimeMs);
        }

        private class MethodTraceInfo
        {
            public string Name { get; }
            public string ClassName { get; }
            public Stopwatch Stopwatch { get; } = new();
            public List<MethodTraceInfo> InnerMethods { get; } = new();
            public long TimeMs => Stopwatch.ElapsedMilliseconds;

            public MethodTraceInfo(string name, string className)
            {
                Name = name;
                ClassName = className;
            }
        }

        private readonly Dictionary<int, ThreadTraceInfo> _threads = new();
        private readonly object _lock = new();

        public void StartTrace()
        {
            lock (_lock)
            {
                var threadId = Thread.CurrentThread.ManagedThreadId;
                if (!_threads.TryGetValue(threadId, out var threadInfo))
                {
                    threadInfo = new ThreadTraceInfo();
                    _threads[threadId] = threadInfo;
                }

                var stackTrace = new StackTrace();
                var method = stackTrace.GetFrame(1)?.GetMethod();
                var methodName = method?.Name ?? "UnknownMethod";
                var className = method?.DeclaringType?.Name ?? "UnknownClass";

                var methodInfo = new MethodTraceInfo(methodName, className);

                if (threadInfo.CallStack.Count == 0)
                {
                    threadInfo.RootMethods.Add(methodInfo);
                }
                else
                {
                    var parent = threadInfo.CallStack.Peek();
                    parent.InnerMethods.Add(methodInfo);
                }

                threadInfo.CallStack.Push(methodInfo);
                methodInfo.Stopwatch.Start();
            }
        }

        public void StopTrace()
        {
            lock (_lock)
            {
                var threadId = Thread.CurrentThread.ManagedThreadId;
                if (!_threads.TryGetValue(threadId, out var threadInfo))
                {
                    throw new InvalidOperationException("StopTrace called without matching StartTrace");
                }

                if (threadInfo.CallStack.Count == 0)
                {
                    throw new InvalidOperationException("StopTrace called without matching StartTrace");
                }

                var methodInfo = threadInfo.CallStack.Pop();
                methodInfo.Stopwatch.Stop();
            }
        }

        public TraceResult GetTraceResult()
        {
            lock (_lock)
            {
                var threadResults = _threads.Select(kvp => 
                {
                    var threadId = kvp.Key;
                    var threadInfo = kvp.Value;
                    
                    var methodResults = ConvertMethods(threadInfo.RootMethods);
                    
                    return new ThreadTraceResult(threadId, threadInfo.TotalTime, methodResults);
                }).ToList();

                return new TraceResult(threadResults);
            }
        }

        private List<MethodTraceResult> ConvertMethods(List<MethodTraceInfo> methods)
        {
            return methods.Select(m => new MethodTraceResult(
                m.Name,
                m.ClassName,
                m.TimeMs,
                ConvertMethods(m.InnerMethods)
            )).ToList();
        }
    }
}