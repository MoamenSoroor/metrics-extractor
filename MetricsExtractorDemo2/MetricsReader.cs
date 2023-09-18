using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml;
using System.Linq;
using System;
using Newtonsoft.Json;
using Microsoft.ApplicationInsights;
using System.Xml.XPath;
using System.Reflection;
using System.IO;

namespace MetricsExtractorDemo2
{
    public static class MetricsReader
    {

        public static Dictionary<string, MemberInfoWithMetrics> ReadMetricsFromXml(string pathToXml)
        {
            //IEnumerable<string> grandChildData =
            //from el in StreamRootChildDoc(pathToXml)
            ////where (int)el.Attribute("Key") > 1
            //select (string)el.Element("GrandChild");
            var metricsXML = XElement.Load(pathToXml);
            //var data = StreamRootChildDoc(pathToXml)
            Console.WriteLine("name: " + metricsXML.Name);
            //Console.WriteLine("name: " + metricsXML.Element("Targets").Name);
            //Console.WriteLine("name: " + metricsXML.Element("Targets").Descendants("Assembly").First().Attribute("Name"));

            string[] importantParents = new[] { "Namespaces", "Types", "Metrics", "Members", "Assembly" };
            
            //Dictionary<string, MemberInfoWithMetrics> metrics = new Dictionary<string, MemberInfoWithMetrics>();

            Dictionary<string, MemberInfoWithMetrics> metrics  = metricsXML.Element("Targets")
                .Descendants("Metrics")
                .Select(metric => new MemberInfoWithMetrics()
                {
                    MetricsInfo = metric.Descendants()
                    .Select(el => new MetricInfo(el.Attribute("Name").Value, el.Attribute("Value").Value)).ToList(),
                    MemberInfo = GetMemberInfo(new MemberInfo(), metric)
                })
                //.GroupBy(g => g.MemberInfo.KeyFullName).Select(g => new { K = g.Key, Count = g.Count() });
            .ToDictionary(dic=> dic.MemberInfo.KeyFullName,dic=> dic);


            //var data = metricsXML.Element("Targets")
            //.Descendants("Metric")
            //.Select(metric =>
            //    new MetricInfo(
            //     metric.Attribute("Name").Value
            //    , metric.Attribute("Value").Value
            //    , GetMemberInfo(new MemberInfo(), metric)
            //    )
            //).ToDictionary(dic => dic.MemberInfo.KeyFullName, dic => dic);



            //var data = metricsXML.Elements("Assembly")
            //    .Where(el=> el.Parent != null && el.Parent.Name == "Namespace" && el.Name == "Metrics")
            //    .SelectMany(el=>  el.Elements("Metrics"))
            //    .Select(m =>
            //        m.Elements("Metric")
            //        .Select(metric=> 
            //        new
            //        {   MetricsObject = metric.Parent.Parent.Attribute("Name").Value,
            //            MetricKey = metric.Attribute("Name").Value,
            //            MetricValue = metric.Attribute("Value").Value
            //        })
            //    );




            //.Select(m=> 
            //    new
            //    {
            //        MaintainabilityIndex = m.Element("Metric").Attribute("Name") == "MaintainabilityIndex",
            //        //("MaintainabilityIndex")?.Value,
            //        CyclomaticComplexity = m.Element("Metric").Attribute("Name") == "CyclomaticComplexity",
            //        //("CyclomaticComplexity")?.Value,
            //        ClassCoupling = m.Element("Metric").Attribute("Name") == "ClassCoupling",
            //        //("ClassCoupling")?.Value,
            //        DepthOfInheritance = m.Element("Metric").Attribute("Name") == "DepthOfInheritance",
            //        //("DepthOfInheritance")?.Value,
            //        SourceLines = m.Element("Metric").Attribute("Name") == "SourceLines",
            //        //("SourceLines")?.Value,
            //        ExecutableLines = m.Element("Metric").Attribute("Name") == "ExecutableLines",
            //        //("ExecutableLines")?.Value,
            //    }
            //);
            string fileName = $@"output-{DateTime.Now.ToFileTime()}.json";
            foreach (var obj in metrics)
            {
                var x = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
                File.AppendAllText(fileName, x);
                Console.WriteLine(x);
            }

            return metrics;
        }



        static string[] members = new[] { "Method", "Field", "Property" };
        static string[] parents = new[] { "Target", "Namespace", "NamedType" };

        private static MemberInfo GetMemberInfo(MemberInfo keyName, XElement element)
        {
            if (element == null || element.Name == "Targets") return keyName;

            else if (members.Any(s => s == element.Name))
            {
                //string val = !string.IsNullOrWhiteSpace(keyName.KeyFullName) ? "::" + keyName : keyName.KeyFullName;
                if(string.IsNullOrWhiteSpace(keyName.KeyFullName))
                {
                    keyName.KeyFullName = element.Attribute("Name").Value;// + val;
                    keyName.MemberType = element.Name.ToString();
                    keyName.File = element.Attribute("File").Value;
                    keyName.Line = element.Attribute("Line").Value;
                }

                return GetMemberInfo(keyName, element.Parent);
            }
            else if (parents.Any(s => s == element.Name))
            {
                string val = !string.IsNullOrWhiteSpace(keyName.KeyFullName) ? "::" + keyName.KeyFullName : string.Empty;
                keyName.KeyFullName = element.Attribute("Name").Value + val;
                string member = !string.IsNullOrWhiteSpace(keyName.MemberType) ? keyName.MemberType : element.Name.ToString();
                keyName.MemberType = member;
                return GetMemberInfo(keyName, element.Parent);
            }
            else
            {
                return GetMemberInfo(keyName, element.Parent);

            }

        }


        private static IEnumerable<XElement> StreamRootChildDoc(string uri)
        {
            using (XmlReader reader = XmlReader.Create(uri))
            {
                reader.MoveToContent();

                // Parse the file and return each of the nodes.
                while (!reader.EOF)
                {
                    if (reader.NodeType == XmlNodeType.Element /*&& reader.Name == "Namespace"*/)
                    {
                        XElement el = XElement.ReadFrom(reader) as XElement;
                        if (el != null)
                            yield return el;
                    }
                    else
                    {
                        reader.Read();
                    }
                }
            }
        }

        //static string [] toSkip = new [] { "Namespaces" , "Types" , "Metrics" , "Members" , "Assembly" };
        //static string [] toPrint = new [] { "Target", "Namespace", "NamedType", "Method", "Field" , "Property" };
        //public static string GetKey(string keyName, XElement element)
        //{
        //    if (element == null || element.Name == "Targets" ) return keyName;

        //    else if (toPrint.Any(s=> s == element.Name))
        //    {
        //        string val = !string.IsNullOrWhiteSpace(keyName) ? "::" + keyName : string.Empty;
        //        return GetKey(element.Attribute("Name") + val , element.Parent);
        //    }
        //    else 
        //    {
        //        return GetKey(keyName, element.Parent);

        //    }

        //}

    }


}
