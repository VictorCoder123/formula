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

        public void AddProperty(string key, string value, string propKey, string propValue)
        {
            var source = NewTraversal(graph).V().Has(key, value).Property(propKey, propValue).ToList();
        }

        public void AddTypeVertex(string type)
        {
            AddVertex("meta", type);
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
                AddEdge("meta", "String", "value", value, "type");
            }
            else
            {
                AddEdge("meta", "Integer", "value", value, "type");
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
            AddEdge("meta", type, "id", id, "type");
        }

        public void Test1()
        {
            // Query: A(hi), F(a, b), H(a, a)
            var list = NewTraversal(graph).V()
                      .Match<Vertex>(
                          __.As("x").Has("meta", "A")
                         
                      )
                      .Select<Vertex>("x").Values<string>("meta").ToList();

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
                          //__.As("f").Out("type").Has("meta", "F"),
                          //__.As("h").Out("type").Has("meta", "H"),
                          //__.As("a").Out("type").Has("meta", "A"),
                          //__.As("b").Out("type").Has("meta", "B"),

                          //__.As("hi").Out("type").Has("meta", "Integer"),
                          //__.As("newa").Out("type").Has("meta", "A"),


                          __.As("a").Out("ARG_" + 0).Has("type", "F").As("0_i_F"), // F
                          __.As("0_i_F").Has("type", "F").In("ARG_" + 0).As("a"),

                          __.As("a").Out("ARG_" + 0).Has("type", "H").As("0_i_H"), // H
                          __.As("0_i_H").Has("type", "H").In("ARG_" + 0).As("a"),

                          __.As("a").Out("ARG_" + 1).Has("type", "H").As("0_i_H"), // H
                          __.As("0_i_H").Has("type", "H").In("ARG_" + 1).As("a"),

                          __.As("b").Out("ARG_" + 1).Has("type", "F").As("0_i_F"),  // F
                          __.As("0_i_F").Has("type", "F").In("ARG_" + 1).As("b")
                          
                          //__.As("hi").Out("ARG_" + 0).Has("type", "A").As("newa"),
                          //__.As("newa").Has("type", "A").In("ARG_" + 0).As("hi")

                      )
                      .Select<Vertex>("a").Values<String>("id").ToList();

            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }

        public void Test3()
        {
            // Query: q :- A(x).
            var list = NewTraversal(graph).V()
                      .Match<Vertex>(
                        __.As("a").Out("ARG_0").Has("type", "H").As("x"),
                        __.As("x").Has("type", "H").In("ARG_0").As("a"),
                        __.As("b").Out("ARG_1").Has("type", "H").As("x"),
                        __.As("x").Has("type", "H").In("ARG_1").As("b"),
                        __.As("b").Out("ARG_0").Has("type", "H").As("y"),
                        __.As("y").Has("type", "H").In("ARG_0").As("b"),
                        //__.Dedup("x", "y")
                        __.Where("x", P.Neq("y"))
                      )
                      .Select<Vertex>("a").Values<string>("id").ToList();

            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }

        public void Test4()
        {
            //var list = NewTraversal(graph).V().Has("type", "D").Values<string>("id").ToList();
            var list = NewTraversal(graph).V()
                      .Match<Vertex>(
                        __.As("a").Out("ARG_0").Has("type", "D").As("0_instance_of_D"),
                        __.As("0_instance_of_D").Has("type", "D").In("ARG_0").As("a")
                        //__.As("b").Out("ARG_1").Has("type", "D").As("0_instance_of_D"),
                        //__.As("0_instance_of_D").Has("type", "D").In("ARG_1").As("b")
                      )
                      .Select<Vertex>("a").Values<string>("value").ToList();


            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }

        public void Test5()
        {
            var list = NewTraversal(graph).V().Has("value", "4").Values<string>("value").ToList();

            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }

    }
}
