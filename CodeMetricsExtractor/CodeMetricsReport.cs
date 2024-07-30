using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CodeMetricsExtractor
{

    public class CodeMetricsReport
    {
        [JsonPropertyName("Version")]
        public string Version { get; set; }

        [JsonPropertyName("Targets")]
        public List<Target> Targets { get; set; }
    }

    public class Target
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("MetricData")]
        public MetricData MetricData { get; set; }
    }

    public class MetricData
    {
        [JsonPropertyName("Kind")]
        public string Kind { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("Metrics")]
        public Dictionary<string, string> Metrics { get; set; }

        [JsonPropertyName("Children")]
        public List<MetricData> Children { get; set; }
    }

    public enum MemberType
    {
        Assembly, Namespace, NamedType, Method, Field, Property
    }

    public class MemberInfoWithMetrics
    {
        public List<MetricInfo> MetricsInfo { get; set; }
        public MemberInfo MemberInfo { get; set; }
    }

    public class MemberInfo
    {
        public MemberInfo() { }

        public MemberInfo(string keyFullName, string memberType)
        {
            KeyFullName = keyFullName;
            MemberType = memberType;
            File = Line = null;
        }

        public MemberInfo(string keyFullName, string memberType, string file, string line)
        {
            KeyFullName = keyFullName;
            MemberType = memberType;
            File = file;
            Line = line;
        }

        public string KeyFullName { get; set; }
        public string MemberType { get; set; }
        public string File { get; set; }
        public string Line { get; set; }

        public List<ReferenceInfo> References { get; set; } = new List<ReferenceInfo>();

    }

    public class MetricInfo
    {
        public MetricInfo(string metricName, string metricValue)
        {
            MetricName = metricName;
            MetricValue = metricValue;
        }

        public string MetricName { get; set; }
        public string MetricValue { get; set; }
    }


    public class ReferenceInfo
    {
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
        public string ContainingMethodName { get; set; }
        public string ContainingTypeName { get; set; }
    }

}
