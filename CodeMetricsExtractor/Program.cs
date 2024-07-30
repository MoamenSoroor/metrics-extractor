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

        

        static string runId = Guid.NewGuid().ToString();
        public static async Task Main(string[] args)
        {
            try
            {
                string solutionFilePath = @"D:\ADSHotFix9\ADSMedical\HMIS_Medical.sln";
                bool silent = false;

                if (!MSBuildLocator.IsRegistered)
                    MSBuildLocator.RegisterDefaults();

                Console.WriteLine("Opening the Solution...");
                using var workspace = MSBuildWorkspace.Create();

                var solution = await workspace.OpenSolutionAsync(solutionFilePath, cancellationToken: CancellationToken.None).ConfigureAwait(false);

                var toPlayProjects = solution.Projects.Where(p=> p.AssemblyName.Contains("PatientProblems.BL")).ToList();
                Console.WriteLine("Start Extracting Metrics for selected projects...");
                Console.WriteLine($"Projects: {string.Join(",", toPlayProjects)}");

                (ImmutableArray<(Project, CodeAnalysisMetricData)>, ErrorCode error) metrics =
                    await CalculateMetrics.GetProjectsMetricDatasAsync(toPlayProjects, silent, CancellationToken.None);

                if (metrics.error != ErrorCode.None)
                {
                    Console.Error.WriteLine(metrics.error);
                    return;
                }

                ExtractFlattenMetrics(metrics.Item1,silent);

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

            string outputFile = CreateOutputFile("full-solution");

            System.IO.File.WriteAllText(outputFile, result);
            Console.WriteLine($"Output File: {outputFile}");

        }


        private static void ExtractFlattenMetrics(ImmutableArray<(Project project, CodeAnalysisMetricData projectMetrics)> data, bool silent)
        {
            foreach (var oneProjectMetrics in data)
            {
                var flatternMetrics = MetricsOutputConverter.FlattenMetrics(oneProjectMetrics.project, oneProjectMetrics.projectMetrics, silent);

                string toOutput = JsonSerializer.Serialize(flatternMetrics, new JsonSerializerOptions { WriteIndented = true });

                string outputFile = CreateOutputFile(oneProjectMetrics.project.Name);

                System.IO.File.WriteAllText(outputFile, toOutput);

                Console.WriteLine($"Output File: {outputFile}");

            }

        }

        private static string CreateOutputFile(string projectName)
        {
            string exeDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "metrics-output", runId);

            if (!Directory.Exists(exeDirectory))
            {
                Directory.CreateDirectory(exeDirectory);
            }

            string outputFile = Path.Combine(exeDirectory, $"{projectName}-metrics-{DateTime.Now.Ticks}.json");
            return outputFile;
        }
    }


}
