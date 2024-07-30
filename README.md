# Code Metrics and Method References Extractor

## Overview

This project extracts code metrics from a specified Visual Studio solution, including maintainability index, cyclomatic complexity, class coupling, depth of inheritance, source lines, executable lines, and references information for each method.

## Prerequisites

- .NET Core SDK
- Visual Studio solution with projects containing C# code

## How to Use

### Step 1: Prepare the Environment

Ensure that the .NET Core SDK is installed and the Visual Studio solution file path is correctly specified in the code.

### Step 2: Running the Code

1. Clone the repository.
2. Navigate to the directory containing the `Main` method in the terminal.
3. Execute the following command:

```sh
dotnet run
```

This will start the process of extracting metrics and generating output files.

### Step 3: Configuring the Solution File Path

Modify the `solutionFilePath` variable in the `Main` method to point to your solution file:

```csharp
string solutionFilePath = @"D:\ADSHotFix9\ADSMedical\HMIS_Medical.sln";
```

### Step 4: Silent Mode (Optional)

If you prefer not to see console output, set the `silent` variable to `true`:

```csharp
bool silent = true;
```

## Output

The code will generate JSON files containing the extracted metrics for each project. These files will be saved in the `metrics-output` directory within the execution directory, named based on the project name and timestamp.

### Example Output

The JSON output will include metrics and references information in the following structure:

```json
{
  "ProjectName::Namespace::ClassName::MethodName": {
    "MetricsInfo": [
      {
        "MetricName": "MaintainabilityIndex",
        "MetricValue": "85"
      },
      {
        "MetricName": "CyclomaticComplexity",
        "MetricValue": "5"
      },
      // Additional metrics...
    ],
    "MemberInfo": {
      "KeyFullName": "ProjectName::Namespace::ClassName::MethodName",
      "MemberType": "Method",
      "File": "Path/To/File.cs",
      "Line": "42",
      "References": [
        {
          "FilePath": "Path/To/ReferenceFile.cs",
          "LineNumber": 32,
          "ColumnNumber": 15,
          "ContainingMethodName": "ReferencingMethod",
          "ContainingTypeName": "ReferencingType"
        }
        // Additional references...
      ]
    }
  }
  // Additional members...
}
```

### Metrics Information

Each method, class, or member will include the following metrics:

- MaintainabilityIndex
- CyclomaticComplexity
- ClassCoupling
- DepthOfInheritance
- SourceLines
- ExecutableLines

### References Information

For each method, the output includes a list of references, each containing:

- FilePath: The path to the file containing the reference.
- LineNumber: The line number where the reference is found.
- ColumnNumber: The column number where the reference is found.
- ContainingMethodName: The name of the method containing the reference.
- ContainingTypeName: The name of the type containing the reference.

## Error Handling

Any errors encountered during the execution will be printed to the console. Ensure the solution file path is correct and that the necessary dependencies are installed.

## Conclusion

This project provides a robust way to extract and analyze code metrics for C# projects within a Visual Studio solution. Customize the code as needed to fit specific requirements and enhance the metrics extraction process.
