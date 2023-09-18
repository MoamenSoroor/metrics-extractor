# Extract Metrics Demo

## Important
To extract metrics correctly, 
Please insure of the following
- you installed <a href='https://www.nuget.org/packages/Microsoft.CodeAnalysis.Metrics/'> Microsoft.CodeAnalysis.Metrics</a> package in all the projects you want to analyze.
- you have msbuild defined in environment variables (system path variable)
	* For VS2019 the path was %ProgramFiles(x86)%\Microsoft Visual Studio\2019\<YOUR_VS_EDITION>\MSBuild\Current\Bin

	* For VS2022 the path was %ProgramFiles%\Microsoft Visual Studio\2022\<YOUR_VS_EDITION>\MSBuild\Current\Bin

	* where <YOUR_VS_EDITION> matches the Visual Studio edition that you have installed, i.e., Preview, Community, Professional, Enterprise.

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
each entry in the dictionary has following shape 
the Key is compined of ProjectName::Namespace::TypeName::MemeberName
the value has the metrics information of each member
and the Member key Information.
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