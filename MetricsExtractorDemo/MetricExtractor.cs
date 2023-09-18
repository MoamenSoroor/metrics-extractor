using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MetricsExtractorDemo2
{
    public static class MetricExtractor
    {
        public static Dictionary<string, MemberInfoWithMetrics> Extract(string projectFileFullPath)
        {
            ScriptManager.ExecutePowerShell(projectFileFullPath);

            List<string> existMetricsXmlFiles = ValidateExistanceOfMetricsFile(projectFileFullPath);

            if (existMetricsXmlFiles?.Count == 0) return new Dictionary<string, MemberInfoWithMetrics>(); 

            return MetricsReader.ReadMetricsFromXml(existMetricsXmlFiles.FirstOrDefault());
        }

        //public static List<Dictionary<string, MemberInfoWithMetrics>> ExtractMany(params string [] projectFileFullPath)
        //{
        //    ScriptManager.ExecutePowerShell(projectFileFullPath);

        //    List<string> existMetricsXmlFiles = ValidateExistanceOfMetricsFile(projectFileFullPath);

        //    if (existMetricsXmlFiles?.Count == 0) return new Dictionary<string, MemberInfoWithMetrics>();

        //    foreach (var item in existMetricsXmlFiles)
        //    {
        //        MetricsReader.ReadMetricsFromXml(existMetricsXmlFiles.FirstOrDefault());
        //    }
            
        //}


        private static List<string> ValidateExistanceOfMetricsFile(params string[] projectsFile)
        {
            List<string> metricsSuppossedFiles = projectsFile
            .Select(s => s.Replace(".csproj", ".Metrics.xml"))
            .ToList();

            List<string> existMetricsXmlFiles = new List<string>();
            foreach (var item in metricsSuppossedFiles)
            {
                if (File.Exists(item))
                {
                    existMetricsXmlFiles.Add(item);
                    Console.WriteLine("metrics file generated Successfully at : " + item);
                }
                else
                    Console.WriteLine("Failed to generated Metrics File : " + item);
            }

            return existMetricsXmlFiles;
        }

    }


}
