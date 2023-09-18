using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace MetricsExtractorDemo2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string projectFile = @"D:\Projects\CSharpProjects\GeneticAlgorithm\SalesManProblem.Algorithms\SalesManProblem.Algorithm.csproj";
            //string projectFile2 = @"D:\Projects\CSharpProjects\GeneticAlgorithm\SalesManProblem\SalesManProblem.UI.csproj";
            //string projectFile3 = @"D:\Projects\CSharpProjects\GeneticAlgorithm\SalesManProblem.Tests\SalesManProblem.Tests.csproj";

            //string[] projectsFile = {
            //    projectFile,projectFile2, projectFile3
            //};


            Dictionary<string, MemberInfoWithMetrics>  metrics = MetricExtractor.Extract(projectFile);
            
            // TODO: use the dictionary as you want.


            Console.WriteLine("end");
            Console.ReadLine();
        }

        


    }

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
