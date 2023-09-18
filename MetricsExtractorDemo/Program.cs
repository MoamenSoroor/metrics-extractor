using System;
using System.Collections.Generic;
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


}
