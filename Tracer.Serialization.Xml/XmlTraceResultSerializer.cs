using System.IO;
using System.Xml.Serialization;
using Tracer.Serialization.Abstractions;
using System.Collections.Generic;
using Tracer.Core;

namespace Tracer.Serialization.Xml
{
    // DTO классы для сериализации
    [XmlRoot("root")]
    public class XmlTraceResult
    {
        [XmlArray("threads")]
        [XmlArrayItem("thread")]
        public List<XmlThreadTraceResult> Threads { get; set; } = new();
    }

    public class XmlThreadTraceResult
    {
        [XmlElement("id")]
        public int Id { get; set; }
        
        [XmlElement("time")]
        public string Time { get; set; } = string.Empty;
        
        [XmlArray("methods")]
        [XmlArrayItem("method")]
        public List<XmlMethodTraceResult> Methods { get; set; } = new();
    }

    public class XmlMethodTraceResult
    {
        [XmlElement("name")]
        public string Name { get; set; } = string.Empty;
        
        [XmlElement("class")]
        public string Class { get; set; } = string.Empty;
        
        [XmlElement("time")]
        public string Time { get; set; } = string.Empty;
        
        [XmlArray("methods")]
        [XmlArrayItem("method")]
        public List<XmlMethodTraceResult> Methods { get; set; } = new();
    }

    public class XmlTraceResultSerializer : ITraceResultSerializer
    {
        public string Format => "xml";

        public void Serialize(TraceResult traceResult, Stream to)
        {
            var xmlResult = ConvertToXmlResult(traceResult);
            var serializer = new XmlSerializer(typeof(XmlTraceResult));
            serializer.Serialize(to, xmlResult);
        }

        private XmlTraceResult ConvertToXmlResult(TraceResult traceResult)
        {
            var result = new XmlTraceResult();
            
            foreach (var thread in traceResult.Threads)
            {
                var xmlThread = new XmlThreadTraceResult
                {
                    Id = thread.Id,
                    Time = $"{thread.Time}ms",
                    Methods = ConvertMethods(thread.Methods)
                };
                result.Threads.Add(xmlThread);
            }
            
            return result;
        }

        private List<XmlMethodTraceResult> ConvertMethods(IReadOnlyList<MethodTraceResult> methods)
        {
            var result = new List<XmlMethodTraceResult>();
            
            foreach (var method in methods)
            {
                var xmlMethod = new XmlMethodTraceResult
                {
                    Name = method.Name,
                    Class = method.Class,
                    Time = $"{method.Time}ms",
                    Methods = ConvertMethods(method.Methods)
                };
                result.Add(xmlMethod);
            }
            
            return result;
        }
    }
}