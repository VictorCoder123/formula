namespace Microsoft.Formula.GraphDatabaseEngine
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Gremlin.Net.Structure;
    using Gremlin.Net.Process.Traversal;
    using Gremlin.Net.Driver;
    using Gremlin.Net.Driver.Remote;

    public class GraphDatabaseTest
    {
        public static string[] COLORS = new string[] { "RED", "GREEN", "BLUE", "BLACK", "WHITE", "YELLOW" };
        public static string[] EDGETYPES = new string[] { "contains", "connects", "inherits" };
        private int vNum;
        private int eNum;
        private Graph graph;
        private List<Tuple<int, int>> randomPairs;

        public GraphDatabaseTest(int vNum, int eNum)
        {
            this.vNum = vNum;
            this.eNum = eNum;

            GraphGenerator gen = new GraphGenerator();
            randomPairs = gen.RandomEdges(this.vNum, this.eNum);

            // Remove all vertices and create a random graph.
            this.graph = new Graph();
            var g = NewTraversal(graph).V().Drop().Iterate();
            CreateRandomGraph(true);
        }

        public GraphTraversalSource NewTraversal(Graph graph)
        {
           return graph.Traversal().WithRemote(new DriverRemoteConnection(new GremlinClient(new GremlinServer("localhost", 8182))));
        }

        public void CreateRandomGraph(bool print)
        {
            Stopwatch stopwatch = new Stopwatch();
            Random rnd = new Random();

            stopwatch.Restart();
            // Insert vertices with random color into graph.
            for (int i = 0; i < vNum; i++)
            {
                var color = COLORS[rnd.Next(0, COLORS.Length)];
                var source = NewTraversal(graph);
                source.AddV().Property("guid", i).Property("color", color).ToList();
            }

            // Insert edges with random label into graph.
            foreach (var edge in randomPairs)
            {
                var source = NewTraversal(graph);
                source.V().Has("guid", edge.Item1).As("a")
                 .V().Has("guid", edge.Item2)
                 .AddE(EDGETYPES[rnd.Next(0, EDGETYPES.Length)]).To("a").ToList();
            }
            stopwatch.Stop();
            if (print) Console.WriteLine("The time to create random graph is {0} second(s)", stopwatch.Elapsed.ToString());
        }

        public TimeSpan TestSelfLoop(bool print)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();
            // Find all vertices with self-loop.
            var list = NewTraversal(graph).V()
                      .Match<Vertex>(__.As("a").Out("contains").As("b").Where("a", P.Eq("b")))
                      .Select<Vertex>("a").Values<int>("guid").ToList();
            stopwatch.Stop();
            if (print)
            {
                Console.WriteLine("The time to execute FORMULA rule equivalent query 1 is {0} second(s)", stopwatch.Elapsed);
                foreach (var item in list) Console.WriteLine(item);
            }
            return stopwatch.Elapsed;
        }

        public TimeSpan TestSelfLoopWithDepth(bool print)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();
            // Find all cycles with certain depth like a-b-c-d-a in graph.
            var paths = NewTraversal(graph).V().As("a").Repeat(__.Out().SimplePath()).Times(3).Where(__.Out().As("a")).Dedup().Values<int>("guid").ToList();
            stopwatch.Stop();

            if (print)
            {
                Console.WriteLine("The time to execute FORMULA-rule equivalent query 2 is {0} second(s)", stopwatch.Elapsed);
                var count = 0;
                foreach (var item in paths)
                {
                    // Console.WriteLine(item);
                    count++;
                }
                Console.WriteLine("The number of cycles is {0}", count);
            }

            return stopwatch.Elapsed;
        }

        public TimeSpan TestMatchSimplePattern(bool print)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();
            // Find all subgraphs matching certain pattern.
            var dicts = NewTraversal(graph).V().Match<Vertex>(
                __.As("a").Has("color", "RED").Out().As("b"),
                __.As("b").Has("color", "BLUE").Out().As("c")
            ).Select<int>("a", "b", "c").By("guid").ToList();
            stopwatch.Stop();

            if (print)
            {
                Console.WriteLine("The time to execute FORMULA-rule equivalent query 3 is {0} second(s)", stopwatch.Elapsed);
                foreach (var item in dicts)
                {
                    Console.WriteLine(item.Values);
                }
            }

            return stopwatch.Elapsed;
        }
    }
}
