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
            GremlinTranslator translator = new GremlinTranslator("test.4ml");
            translator.ExportAllDomainToGraphDB(executor);
            //executor.Test2();
            //executor.Test3();
            //executor.Test4();
            //executor.Test5();
            //translator.TranslateQuery(executor, "A(hi), F(a, b), H(a, a)");
            //translator.TranslateQuery(executor, "H(a, b), H(b, c), H(c, d)");
            //string query = "H(a, b), H(b, c), count({s|s is A(x)}) > 1";
            //string query = "H(a, b), H(b, c), H(c, d)";
            //string query = "C(a, b), C(b, c), count({s|s is C(x, y)}) > 1";
            //string query = "count({s | s is C(a, b)}) > 2";
            string query = "C(a, b)";
            Body body = translator.ParseQueryString(query);
            //translator.TranslateQuery(executor, body, new List<string> {"a"});
            //var queryResult = translator.GetQueryResult(executor, body, new List<string> {"a", "b", "c", "d" });
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

        
        /*
        public static void Main(string[] args)
        {
            List<TimeSpan> testSelfLoop = new List<TimeSpan>();
            List<TimeSpan> testSelfLoopWithDepth = new List<TimeSpan>();
            List<TimeSpan> testMatchSimplePattern = new List<TimeSpan>();

            int[] nums = new int[] { 5, 10, 50, 100, 200, 500, 1000, 1500, 2000 };
            foreach (int vNum in nums)
            {
                int eNum = vNum * vNum / 800;
                GraphDatabaseTest test = new GraphDatabaseTest(vNum, eNum);

                TimeSpan t1 = test.TestSelfLoop(false);
                testSelfLoop.Add(t1);

                TimeSpan t2 = test.TestSelfLoopWithDepth(false);
                testSelfLoopWithDepth.Add(t2);

                TimeSpan t3 = test.TestMatchSimplePattern(false);
                testMatchSimplePattern.Add(t3);
            }

            Console.WriteLine("Self-loop Test result");
            testSelfLoop.ForEach(el => {
                Console.WriteLine(el.ToString());
            });

            Console.WriteLine("Self-loop with depth Test result");
            testSelfLoopWithDepth.ForEach(el => {
                Console.WriteLine(el.ToString());
            });

            Console.WriteLine("Match pattern Test result");
            testMatchSimplePattern.ForEach(el => {
                Console.WriteLine(el.ToString());
            });

            string line = Console.ReadLine();
        } */
    }
}
