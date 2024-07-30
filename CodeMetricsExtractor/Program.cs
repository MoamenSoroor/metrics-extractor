//using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using System.Threading;
using System.Text.Json.Serialization;
using CodeMetricsExtractor;
using Microsoft.Build.Locator;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis.CodeMetrics;
using System.Collections.Immutable;
using System.Composition;
using System.Text.Json;

namespace CodeMetricsAnalyzer
{
    class Program
    {
        public static string solutionFilePath;

        //static async Task Main(string[] args)
        //{
        //    solutionFilePath = args.FirstOrDefault()?? "D:\\ADSHotFix9\\HMISBackoffice";
        //    //if (!MSBuildLocator.IsRegistered)
        //    //    MSBuildLocator.RegisterDefaults();
        //    // Create the MSBuild workspace
        //    var _workspace = MSBuildWorkspace.Create(new Dictionary<string, string>
        //    {
        //        { "RestoreForce", "true" } // Force restore NuGet packages
        //    });

        //    // Open the solution and retrieve the projects
        //    Solution solution = await _workspace.OpenSolutionAsync(solutionFilePath);

        //    foreach (var project in solution.Projects)
        //    {
        //        foreach (var document in project.Documents)
        //        {
        //            var model = await document.GetSemanticModelAsync();
        //            var tree = await document.GetSyntaxTreeAsync();
        //            var root = await tree.GetRootAsync();

        //            var metrics = await CalculateMetrics.GetMetricDatasAsync();
        //            Console.WriteLine($"Metrics for {document.Name}:");
        //            Console.WriteLine($"Maintainability Index: {metrics.MaintainabilityIndex}");
        //            Console.WriteLine($"Cyclomatic Complexity: {metrics.CyclomaticComplexity}");
        //            Console.WriteLine($"Class Coupling: {metrics.ClassCoupling}");
        //            Console.WriteLine($"Depth of Inheritance: {metrics.DepthOfInheritance}");
        //            Console.WriteLine($"Source Lines: {metrics.SourceLines}");
        //            Console.WriteLine($"Executable Lines: {metrics.ExecutableLines}");
        //            Console.WriteLine();
        //        }
        //    }
        //}

        


           public static async Task Main(string[] args)
        {
            try

            {
                if (!MSBuildLocator.IsRegistered)
                    MSBuildLocator.RegisterDefaults();
                
                Solution solution = null;
                
                CalculateMetrics.SolutionLoaded += (sln, e) => solution = (Solution)sln;

                //string solutionFilePath = args.FirstOrDefault() ?? @"D:\Work\Workspaces\pacslive\AndalusiaRadiant\AndalusiaRadiant.sln";
                string solutionFilePath = @"D:\ADSHotFix9\HMISBackoffice\HMIS_BackOffice.sln";
                (ImmutableArray<(string, CodeAnalysisMetricData)>, ErrorCode error) metrics =
                    await CalculateMetrics.GetMetricDatasAsync(new List<string>
            { solutionFilePath }, false, CancellationToken.None);

                if (metrics.error != ErrorCode.None)
                {
                    Console.Error.WriteLine(metrics.error);
                    return;
                }

                //ExtractOriginalMetrics(metrics);
                
                MetricsOutputConverter.SetSolutionToExtractMemebersRefs(solution);

                ExtractFlattenMetrics(metrics);

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                System.Environment.Exit(-1);
            }

        }


        private static void ExtractOriginalMetrics((ImmutableArray<(string, CodeAnalysisMetricData)>, ErrorCode error) metrics)
        {
            string result = MetricsOutputConverter.WriteMetricFile(metrics.Item1);

            string outputFile = CreateOutputFile();

            System.IO.File.WriteAllText(outputFile, result);
            Console.WriteLine($"Output File: {outputFile}");

        }


        private static void ExtractFlattenMetrics((ImmutableArray<(string, CodeAnalysisMetricData)>, ErrorCode error) metrics)
        {
            var flatternMetrics = MetricsOutputConverter.FlattenMetrics(metrics.Item1);

            string toOutput = JsonSerializer.Serialize(flatternMetrics, new JsonSerializerOptions { WriteIndented = true });

            string outputFile = CreateOutputFile();

            System.IO.File.WriteAllText(outputFile, toOutput);

            Console.WriteLine($"Output File: {outputFile}");
        }

        private static string CreateOutputFile()
        {
            string exeDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "metrics-output");

            if (!Directory.Exists(exeDirectory))
            {
                Directory.CreateDirectory(exeDirectory);
            }

            string outputFile = Path.Combine(exeDirectory, $"metrics-{DateTime.Now.Ticks}.json");
            return outputFile;
        }
    }


}
