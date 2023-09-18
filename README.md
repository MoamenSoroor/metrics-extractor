# Extract Code Metrics Demo
 a solution to get project code metrics as objects in memory from the result of msbuild
 based on Microsoft docs <a href='https://learn.microsoft.com/en-us/visualstudio/code-quality/how-to-generate-code-metrics-data?view=vs-2022#microsoftcodeanalysismetrics-nuget-package'>How to: Generate code metrics data</a>
## Important
To extract metrics correctly, 
Please ensure the following:
- you installed <a href='https://www.nuget.org/packages/Microsoft.CodeAnalysis.Metrics/'> Microsoft.CodeAnalysis.Metrics</a> package in all the projects you want to analyze.
- you have <a href='https://dotnet.microsoft.com/en-us/download/dotnet/5.0'>Dotnet5 Sdk</a> installed on your machine 
- you have msbuild defined in environment variables (system path variable)
	* For VS2019 the path was %ProgramFiles(x86)%\Microsoft Visual Studio\2019\<YOUR_VS_EDITION>\MSBuild\Current\Bin

	* For VS2022 the path was %ProgramFiles%\Microsoft Visual Studio\2022\<YOUR_VS_EDITION>\MSBuild\Current\Bin

	* where <YOUR_VS_EDITION> matches the Visual Studio edition you installed, i.e., Preview, Community, Professional, Enterprise.
- this solution consumes large memory, so in large projects consider having enough memory before running it!   

## Consume the Metrics
```cs
        static void Main(string[] args)
        {
            string projectFile = @"D:\Projects\CSharpProjects\GeneticAlgorithm\SalesManProblem.Algorithms\SalesManProblem.Algorithm.csproj";
            Dictionary<string, MemberInfoWithMetrics>  metrics = MetricExtractor.Extract(projectFile);
            
            // TODO: use the dictionary as you want.


            Console.WriteLine("end");
            Console.ReadLine();
        }
```


## Shape of the Result Object
each entry in the dictionary has the following shape 

the Key is composed of four sections "ProjectName::Namespace::TypeName::MemberName"
- for project metrics, the key will be only projectName
- for namespace metrics, the key will be "ProjectName::Namespace:"
- For type metrics, the key will be "ProjectName::Namespace::TypeName"
- for members metrics, the key will be "ProjectName::Namespace::TypeName::MemberName"
  
the value has the metrics information of each member
and the Member key Information as the JSON below.
```json
{
  "Key": "SalesManProblem.Algorithm.csproj::SalesManProblem::Extensions",
  "Value": {
    "MetricsInfo": [
      {
        "MetricName": "MaintainabilityIndex",
        "MetricValue": "83"
      },
      {
        "MetricName": "CyclomaticComplexity",
        "MetricValue": "3"
      },
      {
        "MetricName": "ClassCoupling",
        "MetricValue": "3"
      },
      {
        "MetricName": "DepthOfInheritance",
        "MetricValue": "1"
      },
      {
        "MetricName": "SourceLines",
        "MetricValue": "49"
      },
      {
        "MetricName": "ExecutableLines",
        "MetricValue": "6"
      }
    ],
    "MemberInfo": {
      "KeyFullName": "SalesManProblem.Algorithm.csproj::SalesManProblem::Extensions",
      "MemberType": "NamedType",
      "File": null,
      "Line": null
    }
  }
}
```
