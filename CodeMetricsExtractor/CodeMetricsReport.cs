using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public MemberInfoBase MemberInfo { get; set; }
    }

    public class MemberInfoBase
    {
        public MemberInfoBase() { }

        public MemberInfoBase(string keyFullName, ISymbol symbol, List<MetricInfo> metrics)
        {
            var memberType = symbol.Kind.ToString();
            var file = symbol.Locations.FirstOrDefault()?.SourceTree?.FilePath ?? "";
            var line = (symbol.Locations.FirstOrDefault()?.GetLineSpan().StartLinePosition.Line + 1).ToString() ?? "";
            KeyFullName = keyFullName;
            MemberType = memberType;
            File = file;
            Line = line;
            DocumentFolders = !string.IsNullOrWhiteSpace(file)? Path.GetFileName(file): "";

            // Set metric values
            foreach (var metric in metrics)
            {
                SetMetricValue(metric.MetricName, metric.MetricValue);
            }
        }
        public string SolutionName { get; set; }
        public string ModuleName { get; set; }
        public string ProjectName { get; set; }
        public string KeyFullName { get; set; }
        public string MemberType { get; set; }
        public string File { get; set; }
        public string Line { get; set; }
        public string DocumentFolders { get; set; }
        public string DocumentName { get; set; }
        public List<ReferenceInfo> References { get; set; } = new List<ReferenceInfo>();

        // Metric properties
        public string MaintainabilityIndex { get; set; }
        public string CyclomaticComplexity { get; set; }
        public string ClassCoupling { get; set; }
        public string DepthOfInheritance { get; set; }
        public string SourceLines { get; set; }
        public string ExecutableLines { get; set; }

        private void SetMetricValue(string metricName, string value)
        {
            switch (metricName)
            {
                case "MaintainabilityIndex":
                    MaintainabilityIndex = value;
                    break;
                case "CyclomaticComplexity":
                    CyclomaticComplexity = value;
                    break;
                case "ClassCoupling":
                    ClassCoupling = value;
                    break;
                case "DepthOfInheritance":
                    DepthOfInheritance = value;
                    break;
                case "SourceLines":
                    SourceLines = value;
                    break;
                case "ExecutableLines":
                    ExecutableLines = value;
                    break;
                default:
                    throw new ArgumentException($"Unknown metric name: {metricName}");
            }
        }
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



    public class MethodMemberInfo : MemberInfoBase
    {
        

        public string MethodSymbolContainingNamespace { get; set; }

        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public string MethodFullName { get; set; }
        public string MethodSymbolReturnType { get; set; }
        public bool MethodSymbolIsStatic { get; set; }
        public bool MethodSymbolIsSealed { get; set; }
        public bool MethodSymbolIsExtern { get; set; }
        public bool MethodSymbolIsVirtual { get; set; }
        public bool MethodSymbolIsAbstract { get; set; }
        public bool MethodSymbolIsOverride { get; set; }
        public bool MethodSymbolIsGenericMethod { get; set; }
        public bool MethodSymbolIsAsync { get; set; }
        public string MethodBody { get; set; }

        public List<ReferenceInfo> References { get; set; } = new List<ReferenceInfo>();


        public MethodMemberInfo(string keyFullName, ISymbol symbol, List<MetricInfo> metrics)
        {
            var memberType = symbol.Kind.ToString();
            var file = symbol.Locations.FirstOrDefault()?.SourceTree?.FilePath ?? "";
            var line = (symbol.Locations.FirstOrDefault()?.GetLineSpan().StartLinePosition.Line + 1).ToString() ?? "";
            KeyFullName = keyFullName;
            MemberType = memberType;
            File = file;
            Line = line;

            MethodSymbolContainingNamespace = symbol.ContainingNamespace?.ToDisplayString() ?? "";
            DocumentName = symbol.Locations.FirstOrDefault()?.SourceTree?.FilePath ?? "";
            ClassName = symbol.ContainingType?.Name ?? "";
            MethodName = symbol.Name;
            MethodFullName = GetFullyQualifiedName(symbol);
            MethodSymbolReturnType = (symbol as IMethodSymbol)?.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat) ?? "";
            MethodSymbolIsStatic = symbol.IsStatic;
            MethodSymbolIsSealed = symbol.IsSealed;
            MethodSymbolIsExtern = symbol.IsExtern;
            MethodSymbolIsVirtual = symbol.IsVirtual;
            MethodSymbolIsAbstract = symbol.IsAbstract;
            MethodSymbolIsOverride = symbol.IsOverride;
            MethodSymbolIsGenericMethod = (symbol as IMethodSymbol)?.IsGenericMethod ?? false;
            MethodSymbolIsAsync = (symbol as IMethodSymbol)?.IsAsync ?? false;
            MethodBody = GetMethodBody(symbol as IMethodSymbol);
        }

        private static string GetFullyQualifiedName(ISymbol symbol)
        {
            var parts = new List<string>();

            if (symbol.ContainingNamespace != null)
            {
                parts.Add(symbol.ContainingNamespace.ToDisplayString());
            }

            if (symbol.ContainingType != null)
            {
                parts.Add(symbol.ContainingType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
            }

            parts.Add(symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));

            return string.Join(".", parts);
        }

        private static string GetMethodBody(IMethodSymbol methodSymbol)
        {
            if (methodSymbol == null)
                return string.Empty;

            var syntaxReference = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault();
            if (syntaxReference != null)
            {
                var methodDeclaration = syntaxReference.GetSyntax() as MethodDeclarationSyntax;
                if (methodDeclaration != null)
                {
                    return methodDeclaration.Body?.ToFullString() ?? methodDeclaration.ExpressionBody?.ToFullString() ?? string.Empty;
                }
            }
            return string.Empty;
        }
    }


}
