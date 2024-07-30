//using Microsoft.Build.Locator;
namespace CodeMetricsAnalyzer
{
    public enum ErrorCode
    {
        None,
        Usage,
        FileNotExists,
        RequiresProjectOrSolution,
        NotASolution,
        NotASupportedProject,
        InvalidOutputFile,
        ComputeException,
        WriteException
    }


}
