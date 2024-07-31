using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeMetrics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Newtonsoft.Json;

namespace CodeMetricsExtractor
{
    internal static partial class MetricsOutputConverter
    {
        private const string Version = "1.0";
        public static CodeMetricsReport GetMetricsReport(ImmutableArray<(string, CodeAnalysisMetricData)> data)
        {
            var report = new CodeMetricsReport
            {
                Version = Version,
                Targets = data.Select(kvp => new Target
                {
                    Name = Path.GetFileName(kvp.Item1),
                    MetricData = ConvertMetricData(kvp.Item2)
                }).ToList()
            };

            return report;
        }

        static JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented
        };
        public static string WriteMetricFile(ImmutableArray<(string, CodeAnalysisMetricData)> data)
        {
            var report = new CodeMetricsReport
            {
                Version = Version,
                Targets = data.Select(kvp => new Target
                {
                    Name = Path.GetFileName(kvp.Item1),
                    MetricData = ConvertMetricData(kvp.Item2)
                }).ToList()
            };

            return JsonConvert.SerializeObject(report, settings);
        }

        private static MetricData ConvertMetricData(CodeAnalysisMetricData data)
        {
            var metricData = new MetricData
            {
                Kind = data.Symbol.Kind.ToString(),
                Name = GetSymbolName(data),
                Metrics = new Dictionary<string, string>
                {
                    { "MaintainabilityIndex", data.MaintainabilityIndex.ToString(CultureInfo.InvariantCulture) },
                    { "CyclomaticComplexity", data.CyclomaticComplexity.ToString(CultureInfo.InvariantCulture) },
                    { "ClassCoupling", data.CoupledNamedTypes.Count.ToString(CultureInfo.InvariantCulture) },
                    { "DepthOfInheritance", data.DepthOfInheritance?.ToString(CultureInfo.InvariantCulture) },
#if LEGACY_CODE_METRICS_MODE
                    { "LinesOfCode", data.ExecutableLines.ToString(CultureInfo.InvariantCulture) }
#else
                    { "SourceLines", data.SourceLines.ToString(CultureInfo.InvariantCulture) },
                    { "ExecutableLines", data.ExecutableLines.ToString(CultureInfo.InvariantCulture) }
#endif
                },
                Children = data.Children.Select(ConvertMetricData).ToList()
            };

            return metricData;
        }

        private static string GetSymbolName(CodeAnalysisMetricData data)
        {
            switch (data.Symbol.Kind)
            {
                case SymbolKind.NamedType:
                    var minimalTypeName = new StringBuilder(data.Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
                    var containingType = data.Symbol.ContainingType;
                    while (containingType != null)
                    {
                        minimalTypeName.Insert(0, ".");
                        minimalTypeName.Insert(0, containingType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
                        containingType = containingType.ContainingType;
                    }
                    return minimalTypeName.ToString();

                case SymbolKind.Method:
                case SymbolKind.Field:
                case SymbolKind.Event:
                case SymbolKind.Property:
                    var location = data.Symbol.Locations.First();
                    return $"{data.Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)} (File: {location.SourceTree?.FilePath ?? "UNKNOWN"}, Line: {(location.GetLineSpan().StartLinePosition.Line + 1).ToString(CultureInfo.InvariantCulture)})";

                default:
                    return data.Symbol.ToDisplayString();
            }
        }

        static readonly string[] toCalcRef = new string[] { "Method", "Event" };
        public static Dictionary<string, MemberInfoWithMetrics> FlattenMetrics(Project project, CodeAnalysisMetricData metricsData, bool silent)
        {
            var flattenedMetrics = new Dictionary<string, MemberInfoWithMetrics>();
            

            var solution = project.Solution;

            string projectName = Path.GetFileName(project.FilePath);
            FlattenMetricData(projectName, metricsData, flattenedMetrics, new List<string>());

            return flattenedMetrics;

            void FlattenMetricData(string projectName, CodeAnalysisMetricData data, Dictionary<string, MemberInfoWithMetrics> flattenedMetrics, List<string> pathSegments)
            {
                pathSegments.Add(data.Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));

                var key = string.Join("::", pathSegments.Prepend(projectName));

                var metrics = new List<MetricInfo>
            {
                new MetricInfo("MaintainabilityIndex", data.MaintainabilityIndex.ToString(CultureInfo.InvariantCulture)),
                new MetricInfo("CyclomaticComplexity", data.CyclomaticComplexity.ToString(CultureInfo.InvariantCulture)),
                new MetricInfo("ClassCoupling", data.CoupledNamedTypes.Count.ToString(CultureInfo.InvariantCulture)),
                new MetricInfo("DepthOfInheritance", data.DepthOfInheritance?.ToString(CultureInfo.InvariantCulture)),
#if LEGACY_CODE_METRICS_MODE
                new MetricInfo("LinesOfCode", data.ExecutableLines.ToString(CultureInfo.InvariantCulture))
#else
                new MetricInfo("SourceLines", data.SourceLines.ToString(CultureInfo.InvariantCulture)),
                new MetricInfo("ExecutableLines", data.ExecutableLines.ToString(CultureInfo.InvariantCulture))
#endif
            };
                var memberType = data.Symbol.Kind.ToString();

                MemberInfoBase memberInfo = null;
                if (data.Symbol is IMethodSymbol methodSymbol)
                {
                    memberInfo = new MethodMemberInfo(key,data.Symbol, null);

                }
                else 
                {
                    memberInfo = new MemberInfoBase(key, data.Symbol, null);

                }

                memberInfo.SolutionName = Path.GetFileName(project.Solution.FilePath);
                memberInfo.ProjectName = Path.GetFileName(project.Name);


                if (toCalcRef.Contains(memberType))
                {
                    if(!silent)
                        Console.WriteLine($"Get References of Member: {key}");

                    var references = GetReferences(data.Symbol, solution).Result;
                    memberInfo.References = references;
                }


                var entry = new MemberInfoWithMetrics
                {
                    MetricsInfo = metrics,
                    MemberInfo = memberInfo
                };

                flattenedMetrics[key] = entry;

                foreach (var child in data.Children)
                {
                    FlattenMetricData(projectName, child, flattenedMetrics, new List<string>(pathSegments));
                }
            }


        }


        public static Dictionary<string, MemberInfoBase> FlattenMetricsToMemeberInfo(Project project, CodeAnalysisMetricData metricsData, bool silent)
        {
            var flattenedMetrics = new Dictionary<string, MemberInfoBase>();


            var solution = project.Solution;

            string projectName = Path.GetFileName(project.FilePath);
            FlattenMetricData(projectName, metricsData, flattenedMetrics, new List<string>());

            return flattenedMetrics;

            void FlattenMetricData(string projectName, CodeAnalysisMetricData data, Dictionary<string, MemberInfoBase> flattenedMetrics, List<string> pathSegments)
            {
                pathSegments.Add(data.Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));

                var key = string.Join("::", pathSegments.Prepend(projectName));

                var metrics = new List<MetricInfo>
            {
                new MetricInfo("MaintainabilityIndex", data.MaintainabilityIndex.ToString(CultureInfo.InvariantCulture)),
                new MetricInfo("CyclomaticComplexity", data.CyclomaticComplexity.ToString(CultureInfo.InvariantCulture)),
                new MetricInfo("ClassCoupling", data.CoupledNamedTypes.Count.ToString(CultureInfo.InvariantCulture)),
                new MetricInfo("DepthOfInheritance", data.DepthOfInheritance?.ToString(CultureInfo.InvariantCulture)),
#if LEGACY_CODE_METRICS_MODE
                new MetricInfo("LinesOfCode", data.ExecutableLines.ToString(CultureInfo.InvariantCulture))
#else
                new MetricInfo("SourceLines", data.SourceLines.ToString(CultureInfo.InvariantCulture)),
                new MetricInfo("ExecutableLines", data.ExecutableLines.ToString(CultureInfo.InvariantCulture))
#endif
            };
                var memberType = data.Symbol.Kind.ToString();

                MemberInfoBase memberInfo = null;
                if (data.Symbol is IMethodSymbol methodSymbol)
                {
                    memberInfo = new MethodMemberInfo(key, data.Symbol, metrics);

                }
                else
                {
                    memberInfo = new MemberInfoBase(key, data.Symbol, metrics);

                }

                memberInfo.SolutionName = Path.GetFileName(project.Solution.FilePath);
                memberInfo.ProjectName = Path.GetFileName(project.Name);


                if (toCalcRef.Contains(memberType))
                {
                    if (!silent)
                        Console.WriteLine($"Get References of Member: {key}");

                    var references = GetReferences(data.Symbol, solution).Result;
                    memberInfo.References = references;
                }


                //var entry = new MemberInfoWithMetrics
                //{
                //    MetricsInfo = metrics,
                //    MemberInfo = memberInfo
                //};

                flattenedMetrics[key] = memberInfo;

                foreach (var child in data.Children)
                {
                    FlattenMetricData(projectName, child, flattenedMetrics, new List<string>(pathSegments));
                }
            }


        }



        private static async Task<List<ReferenceInfo>> GetReferences(ISymbol symbol, Solution solution)
        {
            var references = new List<ReferenceInfo>();

            var findReferencesTasks = SymbolFinder.FindReferencesAsync(symbol, solution);
            var referencesResult = await findReferencesTasks;

            foreach (var referencedSymbol in referencesResult)
            {
                foreach (var location in referencedSymbol.Locations)
                {
                    var referenceLocation = location.Location;
                    var referenceDocument = solution.GetDocument(referenceLocation.SourceTree);
                    var referenceSemanticModel = await referenceDocument.GetSemanticModelAsync();
                    var referenceSyntaxRoot = await referenceLocation.SourceTree.GetRootAsync();
                    var referenceNode = referenceSyntaxRoot.FindNode(referenceLocation.SourceSpan);
                    var containingMethod = referenceNode.AncestorsAndSelf()
                                                        .OfType<MethodDeclarationSyntax>()
                                                        .FirstOrDefault();
                    var containingType = referenceNode.AncestorsAndSelf()
                                                      .OfType<TypeDeclarationSyntax>()
                                                      .FirstOrDefault();

                    string containingMethodName = containingMethod?.Identifier.Text ?? "Unknown Method";
                    string containingTypeName = containingType?.Identifier.Text ?? "Unknown Type";

                    var referenceInfo = new ReferenceInfo
                    {
                        FilePath = referenceLocation.SourceTree.FilePath,
                        LineNumber = referenceLocation.GetLineSpan().StartLinePosition.Line + 1,
                        ColumnNumber = referenceLocation.GetLineSpan().StartLinePosition.Character + 1,
                        ContainingMethodName = containingMethodName,
                        ContainingTypeName = containingTypeName
                    };

                    references.Add(referenceInfo);
                }
            }

            return references;
        }


    }



public static class SymbolExtensions
    {
        public static string ToSimpleMethodSignature(this IMethodSymbol methodSymbol)
        {
            var format = new SymbolDisplayFormat(
                memberOptions: SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeType,
                parameterOptions: SymbolDisplayParameterOptions.IncludeName | SymbolDisplayParameterOptions.IncludeType,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters);

            return methodSymbol.ToDisplayString(format);
        }
    }
}