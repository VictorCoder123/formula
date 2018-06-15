namespace Microsoft.Formula.GraphDatabaseEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Microsoft.Formula.API;
    using Microsoft.Formula.API.ASTQueries;
    using Microsoft.Formula.API.Nodes;
    using Microsoft.Formula.API.Generators;

    public class CommandLine
    {
        public static void Main(string[] args)
        {
            GraphDBExecutor executor = new GraphDBExecutor("localhost", 8182);
            TestCase testcase = new TestCase();
            var queryResult = testcase.Test2(executor);

            // Print out query result in test case.
            foreach(var dict in queryResult)
            {
                foreach (var pair in dict)
                {
                    Console.WriteLine("{0} : {1}", pair.Key, pair.Value);
                }
                Console.WriteLine("----------------------");
            }

            string line = Console.ReadLine();
        }

    }
}
