using System.Collections.Generic;

namespace MetricsExtractorDemo2
{
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
        public MemberInfo()
        {
        }
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
    }

    public class MetricInfo
    {
        //public MetricInfo(string metricName, string metricValue, MemberInfo memberInfo)
        //{
        //    MetricName = metricName;
        //    MetricValue = metricValue;
        //    MemberInfo = memberInfo;
        //}
        public MetricInfo(string metricName, string metricValue)
        {
            MetricName = metricName;
            MetricValue = metricValue;
            //MemberInfo = memberInfo;
        }
        public string MetricName { get; set; }
        public string MetricValue { get; set; }
        //public MemberInfo MemberInfo { get; }
    }
}
