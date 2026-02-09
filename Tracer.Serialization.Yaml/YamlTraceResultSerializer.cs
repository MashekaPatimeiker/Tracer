using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Tracer.Serialization.Abstractions;
using System.Collections.Generic;
using Tracer.Core;

namespace Tracer.Serialization.Yaml
{
    public class YamlTraceResult
    {
        public List<YamlThreadTraceResult> Threads { get; set; } = new();
    }

    public class YamlThreadTraceResult
    {
        public int Id { get; set; }
        public string Time { get; set; } = string.Empty;
        public List<YamlMethodTraceResult> Methods { get; set; } = new();
    }

    public class YamlMethodTraceResult
    {
        public string Name { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public List<YamlMethodTraceResult> Methods { get; set; } = new();
    }

    public class YamlTraceResultSerializer : ITraceResultSerializer
    {
        public string Format => "yaml";

        public void Serialize(TraceResult traceResult, Stream to)
        {
            var yamlResult = ConvertToYamlResult(traceResult);
            
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            
            using var writer = new StreamWriter(to);
            serializer.Serialize(writer, yamlResult);
        }

        private YamlTraceResult ConvertToYamlResult(TraceResult traceResult)
        {
            var result = new YamlTraceResult();
            
            foreach (var thread in traceResult.Threads)
            {
                var yamlThread = new YamlThreadTraceResult
                {
                    Id = thread.Id,
                    Time = $"{thread.Time}ms",
                    Methods = ConvertMethods(thread.Methods)
                };
                result.Threads.Add(yamlThread);
            }
            
            return result;
        }

        private List<YamlMethodTraceResult> ConvertMethods(IReadOnlyList<MethodTraceResult> methods)
        {
            var result = new List<YamlMethodTraceResult>();
            
            foreach (var method in methods)
            {
                var yamlMethod = new YamlMethodTraceResult
                {
                    Name = method.Name,
                    Class = method.Class,
                    Time = $"{method.Time}ms",
                    Methods = ConvertMethods(method.Methods)
                };
                result.Add(yamlMethod);
            }
            
            return result;
        }
    }
}