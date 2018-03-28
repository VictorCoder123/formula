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

        // Add node with single property defined in key/value pair.
        public void AddVertex(string key, object value)
        {
            var source = NewTraversal(graph).AddV()
                .Property(key, value)
                .ToList();
        }

        // Add node with a list of properties in the format of key/value pairs.
        public void AddVertex(List<KeyValuePair<string, object>> properties)
        {
            var source = NewTraversal(graph).AddV();
            foreach (var property in properties)
            {
                source = source.Property(property.Key, property.Value);
            }
            source.ToList();
        }

        public void AddTypeVertex(string type)
        {
            AddVertex("type", type);
        }

        public void AddModelVertex(string id)
        {
            AddVertex("id", id);
        }

        public void AddCnstVertex(string value, bool isString)
        {
            var source = NewTraversal(graph).AddV();
            source = source.Property("value", value);
            source = source.Property("isString", isString);
            source.ToList();
        }

        // Find nodes with specific property and connect them as an edge with dst points to src.
        public void AddEdge(string srcKey, object srcValue, string dstKey, object dstValue, string edgeType)
        {
            var source = NewTraversal(graph);
            var traversal = source.V().Has(srcKey, srcValue).As("a")
                                  .V().Has(dstKey, dstValue)
                                  .AddE(edgeType).To("a").ToList();
        }

        public void AddEdge(KeyValuePair<String, object> srcProp, KeyValuePair<String, object> dstProp, string edgeType)
        {
            AddEdge(srcProp.Key, srcProp.Value, dstProp.Key, dstProp.Value, edgeType);
        }

        // Connect Cnst to FuncTerm as argument.
        public void connectCnstToFuncTerm(string id, object cnst, string edgeType)
        {
            if (cnst.GetType() == typeof(String))
            {
                AddEdge("id", id, "value", (String)cnst, edgeType);  
            }
            else if (cnst.GetType() == typeof(int))
            {
                AddEdge("id", id, "value", (int)cnst, edgeType);
            }
        }

        // Connect Cnst to its type node Integer or String.
        public void connectCnstToType(string value, bool isString)
        {
            if (isString)
            {
                AddEdge("type", "String", "value", value, "type");
            }
            else
            {
                AddEdge("type", "Integer", "value", value, "type");
            }
        }

        // Connect argument FuncTerm to parent FuncTerm. (idy -> idx)
        public void connectFuncTermToFuncTerm(string idx, string idy, string edgeType)
        {
            AddEdge("id", idx, "id", idy, edgeType);
        }

        // Connect FuncTerm to type (id -> type)
        public void connectFuncTermToType(string type, string id)
        {
            AddEdge("type", type, "id", id, "type");
        }

        public void Test1()
        {
            var list = NewTraversal(graph).V().Has("type", "A").Values<String>("type").ToList();

            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }

        public void Test2()
        {
            // Query: A(hi), F(a, b), H(a, a)
            var list = NewTraversal(graph).V()
                      .Match<Vertex>(
                          __.As("a").Out("type").Has("type", "A"),
                          __.As("a").Out("ARG_" + 0).Out("type").Has("type", "F"),
                          __.As("a").Out("type").Has("type", "A"),
                          __.As("a").Out("ARG_" + 0).Out("type").Has("type", "H"),
                          __.As("a").Out("type").Has("type", "A"),
                          __.As("a").Out("ARG_" + 1).Out("type").Has("type", "H"),
                          __.As("b").Out("type").Has("type", "B"),
                          __.As("b").Out("ARG_" + 1).Out("type").Has("type", "F"),
                          __.As("hi").Out("type").Has("type", "Integer"),
                          __.As("hi").Out("ARG_" + 0).Out("type").Has("type", "A")
                      )
                      .Select<Vertex>("a").Values<string>("id").ToList();

            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }

    }
}
