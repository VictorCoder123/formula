namespace Microsoft.Formula.GraphDatabaseEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Gremlin.Net.Structure;
    using Gremlin.Net.Process.Traversal;
    using Gremlin.Net.Driver;
    using Gremlin.Net.Driver.Remote;

    public class GraphDBExecutor
    {
        // Default URL and port to connect GraphDB.
        private string address = "localhost";
        private int port = 8182;
        private Graph graph;

        public GraphDBExecutor(string address, int port)
        {
            // Remove all vertices and create a random graph.
            this.graph = new Graph();
            var g = NewTraversal(graph).V().Drop().Iterate();
        }

        public GraphTraversalSource NewTraversal()
        {
            return NewTraversal(graph);
        }

        public GraphTraversalSource NewTraversal(Graph graph)
        {
            return graph.Traversal().WithRemote(new DriverRemoteConnection(new GremlinClient(new GremlinServer(address, port))));
        }
        
        public void AddVertex(KeyValuePair<string, string> node)
        {
            var source = NewTraversal(graph).AddV()
                .Property("id", node.Key)
                .Property("type", node.Value)
                .ToList();
        }

        public void AddVertex(List<KeyValuePair<string, object>> properties)
        {
            var source = NewTraversal(graph).AddV();
            foreach (var property in properties)
            {
                source = source.Property(property.Key, property.Value);
            }
            source.ToList();
        }

        // Connect Cnst to FuncTerm.
        public void AddEdgeToCnst(string id, object cnst, string edgeType)
        {
            var source = NewTraversal(graph);
            var traversal = source.V().Has("id", id).As("a");

            if (cnst.GetType() == typeof(String))
            {
                traversal.V().Has("value", (String)cnst).AddE(edgeType).To("a").ToList();
            }
            else if (cnst.GetType() == typeof(int))
            {
                traversal.V().Has("value", (int)cnst).AddE(edgeType).To("a").ToList();
            }           
        }

        // Connect argument FuncTerm to parent FuncTerm.
        public void AddEdgeToFuncTerm(string idx, string idy, string edgeType)
        {
            var source = NewTraversal(graph);
            var traversal = source.V().Has("id", idx).As("a")
                  .V().Has("id", idy)
                  .AddE(edgeType).To("a").ToList();
        }

        public void AddVertexProperty(string id, KeyValuePair<string, object> prop)
        {
            var source = NewTraversal(graph);
            source.V().Has("id", id).Property(prop.Key, prop.Value).ToList();
        }

        public void Test1()
        {
            var list = NewTraversal(graph).V().Has("type", "A").Values<String>("id").ToList();

            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }

        public void Test2()
        {
            var list = NewTraversal(graph).V()
                      .Match<Vertex>(__.As("a").Out("contains").As("b").Where("a", P.Eq("b")))
                      .Select<Vertex>("a").Values<int>("guid").ToList();

            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }

    }
}
