using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tracer.Serialization.Abstractions;
using System.Collections.Generic;
using Tracer.Core;

namespace Tracer.Serialization.Json
{
    // DTO классы для сериализации
    public class JsonTraceResult
    {
        [JsonPropertyName("threads")]
        public List<JsonThreadTraceResult> Threads { get; set; } = new();
    }

    public class JsonThreadTraceResult
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("time")]
        public string Time { get; set; } = string.Empty;
        
        [JsonPropertyName("methods")]
        public List<JsonMethodTraceResult> Methods { get; set; } = new();
    }

    public class JsonMethodTraceResult
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("class")]
        public string Class { get; set; } = string.Empty;
        
        [JsonPropertyName("time")]
        public string Time { get; set; } = string.Empty;
        
        [JsonPropertyName("methods")]
        public List<JsonMethodTraceResult> Methods { get; set; } = new();
    }

    public class JsonTraceResultSerializer : ITraceResultSerializer
    {
        public string Format => "json";

        public void Serialize(TraceResult traceResult, Stream to)
        {
            var jsonResult = ConvertToJsonResult(traceResult);
            
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            JsonSerializer.Serialize(to, jsonResult, options);
        }

        private JsonTraceResult ConvertToJsonResult(TraceResult traceResult)
        {
            var result = new JsonTraceResult();
            
            foreach (var thread in traceResult.Threads)
            {
                var jsonThread = new JsonThreadTraceResult
                {
                    Id = thread.Id,
                    Time = $"{thread.Time}ms",
                    Methods = ConvertMethods(thread.Methods)
                };
                result.Threads.Add(jsonThread);
            }
            
            return result;
        }

        private List<JsonMethodTraceResult> ConvertMethods(IReadOnlyList<MethodTraceResult> methods)
        {
            var result = new List<JsonMethodTraceResult>();
            
            foreach (var method in methods)
            {
                var jsonMethod = new JsonMethodTraceResult
                {
                    Name = method.Name,
                    Class = method.Class,
                    Time = $"{method.Time}ms",
                    Methods = ConvertMethods(method.Methods)
                };
                result.Add(jsonMethod);
            }
            
            return result;
        }
    }
}