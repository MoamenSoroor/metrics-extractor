//using Microsoft.Build.Locator;
using System;
using Microsoft.CodeAnalysis.MSBuild;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeMetrics;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace CodeMetricsAnalyzer
{
    public partial class CalculateMetrics
    {
        public static event EventHandler SolutionLoaded;
        public static async Task<(ImmutableArray<(string, CodeAnalysisMetricData)>, ErrorCode)> GetMetricDatasAsync(List<string> projectsOrSolutions, bool quiet, CancellationToken cancellationToken)
        {

            var builder = ImmutableArray.CreateBuilder<(string, CodeAnalysisMetricData)>();

            try
            {
                using (var workspace = MSBuildWorkspace.Create())
                {
                    foreach (var projectOrSolution in projectsOrSolutions)
                    {
                        if (projectOrSolution.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
                        {
                            await computeSolutionMetricDataAsync(workspace, projectOrSolution, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            Debug.Assert(projectOrSolution.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) ||
                                projectOrSolution.EndsWith(".vbproj", StringComparison.OrdinalIgnoreCase));
                            await computeProjectMetricDataAsync(workspace, projectOrSolution, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }

                return (builder.ToImmutable(), ErrorCode.None);
            }
#pragma warning disable CA1031 // Do not catch general exception types - gracefully catch exceptions and log them to the console and output file.
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return (ImmutableArray<(string, CodeAnalysisMetricData)>.Empty, ErrorCode.ComputeException);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            async Task computeProjectMetricDataAsync(MSBuildWorkspace workspace, string projectFile, CancellationToken cancellation)
            {
                cancellation.ThrowIfCancellationRequested();
                if (!quiet)
                {
                    Console.WriteLine($"Loading {Path.GetFileName(projectFile)}...");
                }

                var project = await workspace.OpenProjectAsync(projectFile, cancellationToken: CancellationToken.None).ConfigureAwait(false);

                if (!quiet)
                {
                    Console.WriteLine($"Computing code metrics for {Path.GetFileName(projectFile)}...");
                }

                if (!project.SupportsCompilation)
                {
                    throw new NotSupportedException("Project must support compilation.");
                }

                cancellation.ThrowIfCancellationRequested();
                var compilation = await project.GetCompilationAsync(CancellationToken.None).ConfigureAwait(false);
                var metricData = await CodeAnalysisMetricData.ComputeAsync(compilation!.Assembly, new CodeMetricsAnalysisContext(compilation, CancellationToken.None)).ConfigureAwait(false);
                builder.Add((projectFile, metricData));
            }

            async Task computeSolutionMetricDataAsync(MSBuildWorkspace workspace, string solutionFile, CancellationToken cancellation)
            {
                cancellation.ThrowIfCancellationRequested();
                if (!quiet)
                {
                    Console.WriteLine($"Loading {Path.GetFileName(solutionFile)}...");
                }

                var solution = await workspace.OpenSolutionAsync(solutionFile, cancellationToken: CancellationToken.None).ConfigureAwait(false);
                SolutionLoaded.Invoke(solution,new EventArgs());
                if (!quiet)
                {
                    Console.WriteLine($"Computing code metrics for {Path.GetFileName(solutionFile)}...");
                }

                foreach (var project in solution.Projects)
                {
                    if (!quiet)
                    {
                        Console.WriteLine($"    Computing code metrics for {Path.GetFileName(project.FilePath)}...");
                    }

                    if (!project.SupportsCompilation)
                    {
                        throw new NotSupportedException("Project must support compilation.");
                    }

                    cancellation.ThrowIfCancellationRequested();
                    var compilation = await project.GetCompilationAsync(CancellationToken.None).ConfigureAwait(false);
                    var metricData = await CodeAnalysisMetricData.ComputeAsync(compilation!.Assembly, new CodeMetricsAnalysisContext(compilation, CancellationToken.None)).ConfigureAwait(false);
                    builder.Add((project.FilePath!, metricData));
                }
            }
        }
    }

    
}
