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
            GremlinTranslator translator = new GremlinTranslator("test2.4ml");
            translator.ExportAllDomainToGraphDB(executor);
            string query = "C(a, b)";
            Body body = translator.ParseQueryString(query); 
            DomainStore store = translator.DomainStores["TestModelPlus"];
            var queryResult = translator.GetQueryResult(executor, store, body, new List<string> { "a", "b" });

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
