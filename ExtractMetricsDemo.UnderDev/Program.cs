//using Metrics;
//using Microsoft.CodeAnalysis.CodeMetrics;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;

//namespace ExtractMetricsDemo
//{
//    internal class Program
//    {
//        static async Task Main(string[] args)
//        {
//            try
//            {
//                Console.WriteLine("Start");

//                List<string> data = new List<string>()
//                {
//                    @"D:\\Projects\\CSharpProjects\\DesignPatternsCourse\\DesignPatternsCourse.sln"
//                };
//                var result = await MetricsExtractor.GetMetricDatasAsync(data, cancellationToken: CancellationToken.None);
//                if(result.Item2 != ErrorCode.None)
//                    Console.WriteLine("Error Code: "+result.Item2.ToString());
//                Console.WriteLine("Code: " + result.Item2.ToString());

//                foreach (var item in result.Item1)
//                {
//                    Console.WriteLine("File Path: "+ item.Item1);
//                    Console.WriteLine("Metrics: "+ item.Item2.ToString());
//                }

//                string str = JsonConvert.SerializeObject(result.Item1);
//                System.IO.File.WriteAllText("output-metrics-" + DateTime.Now.ToFileTimeUtc()+".json", str);

//                Console.WriteLine("End");
//            }
//            catch (Exception ex)
//            {
//                Console.Error.WriteLine(ex.ToString());
//                Console.Error.WriteLine(ex.StackTrace);
//                Console.WriteLine("End with failure");

//            }
//        }
//    }
//}
