using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace MetricsExtractorDemo2
{
    public static class ScriptManager
    {

        public static string ExecutePowerShell(params string[] projectsFiles)
        {
            try
            {
                List<string> paths = projectsFiles.Select(s => Path.GetDirectoryName(s)).ToList();

                StringBuilder scriptBuilder = new StringBuilder();
                foreach (var item in paths)
                {
                    string setLoc = @$"set-location '{item}'";
                    scriptBuilder.AppendLine(setLoc);
                    scriptBuilder.AppendLine(@"msbuild /t:Metrics");
                }

                string script = scriptBuilder.ToString();

                Console.WriteLine(script);
                Console.WriteLine("==================================================================");

                Execute(script);





                //var processes = ps.Invoke();
                //Console.WriteLine(string.Join(Environment.NewLine, processes));
                return "done";
            }
            catch (CommandNotFoundException ex)
            {
                Console.WriteLine("Error ", ex);
                Console.WriteLine(ex.Message);
                return "failed";
            }

        }


        private static void Execute(string command)
        {
            using (var ps = PowerShell.Create())
            {
                var results = ps.AddScript(command).Invoke();
                foreach (var result in results)
                {
                    Console.WriteLine(result.ToString());
                }
            }
        }






        private static string ExecutePowerShellOld(params string[] projectsFiles)
        {
            try
            {
                List<string> paths = projectsFiles.Select(s => Path.GetDirectoryName(s)).ToList();
                using var ps = PowerShell.Create();

                ps.AddCommand("Get-Date");
                foreach (var projectDirectoryPath in paths)
                {
                    //string redirectCommand = @$"cd '{projectDirectoryPath}'";
                    //string buildWithMetricOutput = @$"msbuild /t:Metrics";
                    ps.AddCommand("set-location").AddArgument(projectDirectoryPath);
                    //var process = ps.Invoke();
                    //Console.WriteLine("location command: " + string.Join(Environment.NewLine, process.Select(p=> p.ToString())));

                    ps.AddCommand("pwd");
                    //process = ps.Invoke();
                    //Console.WriteLine("pwd command: " + string.Join(Environment.NewLine, process.Select(p => p.ToString())));
                    ps.AddCommand("msbuild")
                        //.AddArgument($@"/p:DirPath='{projectDirectoryPath}'")
                        .AddArgument("/t:Metrics");

                    var process = ps.Invoke();
                    Console.WriteLine("msbuild command: " + string.Join(Environment.NewLine, process.Select(p => p.ToString())));
                }




                //var processes = ps.Invoke();
                //Console.WriteLine(string.Join(Environment.NewLine, processes));
                return "done";
            }
            catch (CommandNotFoundException ex)
            {
                Console.WriteLine("Error ", ex);
                Console.WriteLine(ex.Message);
                return "failed";
            }

        }


        private static string ExecuteScript(string pathToScript)
        {
            var scriptArguments = "-ExecutionPolicy Bypass -File \"" + pathToScript + "\"";
            var processStartInfo = new ProcessStartInfo("powershell.exe", scriptArguments);
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;

            using var process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            Console.WriteLine(output); // I am invoked using ProcessStartInfoClass!

            return "";
        }

        private static string ExecuteCommand(string command)
        {
            var processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = "powershell.exe";
            processStartInfo.Arguments = $"-Command \"{command}\"";
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;

            using var process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            Console.WriteLine(output);
            return output;
        }
    }


}
