namespace Microsoft.Formula.GraphDatabaseEngine
{
    using System;
    using System.IO;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Microsoft.Formula.API;
    using Microsoft.Formula.API.ASTQueries;
    using Microsoft.Formula.API.Nodes;
    using Microsoft.Formula.API.Generators;

    public class CommandLine
    {
        public void StartGremlinServer()
        {
            // Start Gremlin server by default config.
            string gremlinServerExecutablePath = Path.Combine(Environment.CurrentDirectory, "apache-tinkerpop-gremlin-server-3.3.1\\bin\\gremlin-server.bat");
            Console.WriteLine(gremlinServerExecutablePath);

            ProcessStartInfo processInfo = new ProcessStartInfo(gremlinServerExecutablePath);
            processInfo.CreateNoWindow = false;
            processInfo.UseShellExecute = true;
            processInfo.WindowStyle = ProcessWindowStyle.Normal;

            // *** Redirect the output ***
            //processInfo.RedirectStandardError = true;
            //processInfo.RedirectStandardOutput = true;

            Process process = Process.Start(processInfo);
            process.WaitForExit();
        }

        public static void Main(string[] args)
        { 
            GraphDBExecutor executor = new GraphDBExecutor("localhost", 8182);

            if (args != null && args.Length > 0 && args[0].Split('.')[1] == "4ml")
            {
                string formulaFile = args[0];
                GremlinTranslator translator = new GremlinTranslator(formulaFile);
                translator.ExportAllDomainToGraphDB(executor);

                // Print out all Gremlin commands and write to local file.
                Console.WriteLine(executor.GetGremlinScript());
                executor.WriteGremlinScriptToFile(formulaFile);
            }
            else
            {
                Console.WriteLine(".4ml file has not been specified as argument and program will run a default test.");
                TestCase testcase = new TestCase();
                var queryResult = testcase.Test6(executor);

                // Print out query result in test case.
                foreach (var dict in queryResult)
                {
                    foreach (var pair in dict)
                    {
                        Console.WriteLine("{0} : {1}", pair.Key, pair.Value);
                    }
                    Console.WriteLine("----------------------");
                }
            }
            
            string line = Console.ReadLine();
        }

    }
}
