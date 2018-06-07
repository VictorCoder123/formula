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

    using Microsoft.Formula.API;
    using Microsoft.Formula.API.ASTQueries;
    using Microsoft.Formula.API.Nodes;
    using Microsoft.Formula.Common;
    using Microsoft.Formula.API.Generators;

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

        public int ToInteger(string str)
        {
            Rational r;
            Rational.TryParseDecimal(str, out r);
            int num = (int)r.Numerator;
            return num;
        }

        // Add node with single property defined in key/value pair.
        public void AddVertex(string key, object value)
        {
            var source = NewTraversal(graph).AddV()
                .Property(key, value)
                .ToList();
        }

        public void AddVertex(string key1, object value1, string key2, object value2)
        {
            var source = NewTraversal(graph).AddV()
                .Property(key1, value1)
                .Property(key2, value2)
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

        public void AddProperty(string key, string value, string propKey, string propValue, string domain)
        {
            var source = NewTraversal(graph).V().Has(key, value).Has("domain", domain).Property(propKey, propValue).ToList();
        }

        public void AddDomainVertex(string domain)
        {
            AddVertex("scope", domain, "domain", domain);
        }

        public void AddTypeVertex(string type, string domain)
        {
            AddVertex("meta", type, "domain", domain);
        }

        public void AddModelVertex(string id, string domain)
        {
            AddVertex("id", id, "domain", domain);
        }

        public void AddCnstVertex(string valueString, bool isString, string domain)
        {
            var source = NewTraversal(graph).AddV();
            if (isString)
            {
                source = source.Property("value", valueString);
            }
            else
            {
                int num = ToInteger(valueString);
                source = source.Property("value", num);
            }

            source = source.Property("isString", isString);
            source = source.Property("domain", domain);
            source.ToList();
        }

        // Find nodes with specific property and connect them as an edge with dst points to src. (src -> dst)
        public void AddEdge(string srcKey, object srcValue, string dstKey, object dstValue, string edgeType, string domain)
        {
            var source = NewTraversal(graph);
            var traversal = source.V().Has(dstKey, dstValue).Has("domain", domain).As("a")
                                  .V().Has(srcKey, srcValue).Has("domain", domain)
                                  .AddE(edgeType).To("a").ToList();
        }

        public void AddEdge(KeyValuePair<String, object> srcProp, KeyValuePair<String, object> dstProp, string edgeType, string domain)
        {
            AddEdge(srcProp.Key, srcProp.Value, dstProp.Key, dstProp.Value, edgeType, domain);
        }

        // Connect Cnst to FuncTerm as argument. (FuncTerm -> cnst)
        public void connectFuncTermToCnst(string id, string cnstString, bool isString, string edgeType, string domain)
        {
            if (isString)
            {
                AddEdge("id", id, "value", cnstString, edgeType, domain);  
            }
            else
            {
                int num = ToInteger(cnstString);
                AddEdge("id", id, "value", num, edgeType, domain);
            }
        }

        // Connect Cnst to FuncTerm when numeric value is known.
        public void connectFuncTermToCnst(string id, int value, string edgeType, string domain)
        {
            AddEdge("id", id, "value", value, edgeType, domain);
        }

        // Connect type to its scope node. (type -> domain)
        public void connectTypeToDomain(string type, string domain)
        {
            string edgeLabel = "domain";
            AddEdge("meta", type, "scope", domain, edgeLabel, domain);
        }

        // Connect Cnst to enum type (cnst -> enum type)
        public void connectCnstToEnumType(string value, bool isString, string enumType, string domain)
        {
            if (isString)
            {
                AddEdge("value", value, "meta", enumType, "enum", domain);
            }
            else
            {
                int num = ToInteger(value);
                AddEdge("value", num, "meta", enumType, "enum", domain);
            }
        }

        // Connect sub-type to its union type. (subtype -> type), the label name of edge is "type".
        public void connectSubtypeToType(string subtype, string type, string domain)
        {
            AddEdge("meta", subtype, "meta", type, "type", domain);
        }

        // Connect Cnst to its type node Integer or String. (cnst -> type)
        public void connectCnstToType(string value, bool isString, string domain)
        {
            if (isString)
            {
                AddEdge("value", value, "meta", "String", "type", domain);
            }
            else
            {
                int num = ToInteger(value);
                AddEdge("value", num, "meta", "Integer", "type", domain);
            }
        }

        // Connect type to argument type by auto-generated label (ARG_X) and user defined argument label. (type -> arg type)
        public void connectTypeToArgType(string type, string argType, string edgeType, string edgeLabel, string domain)
        {
            AddEdge("meta", type, "meta", argType, edgeType, domain);
            if (edgeLabel != null)
            {
                AddEdge("meta", type, "meta", argType, edgeLabel, domain);
            }
        }

        // Connect parent FuncTerm to argument FuncTerm. (idx -> idy)
        public void connectFuncTermToFuncTerm(string idx, string idy, string edgeType, string edgeLabel, string domain)
        {
            AddEdge("id", idx, "id", idy, edgeType, domain);
            if (edgeLabel != null)
            {
                AddEdge("id", idx, "id", idy, edgeLabel, domain);
            }
        }

        // Connect FuncTerm to type (id -> type)
        public void connectFuncTermToType(string id, string type, string domain)
        {
            AddEdge("id", id, "meta", type, "type", domain);
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
            var list = NewTraversal(graph).V()
                      .Match<Vertex>(
                        __.As("a").Out("ARG_0").Has("type", "C").As("s"),
                        __.As("s").Has("type", "C").In("ARG_0").As("a"),
                        __.As("b").Out("ARG_1").Has("type", "C").As("s"),
                        __.As("s").Has("type", "C").In("ARG_1").As("b"),
                        __.As("s").Count().As("cnt"),
                        __.Where("a", P.Neq("b"))
                     )//.Where(__.As("s").Count().Is(P.Gt(1)))
                      //.Where("cnt", P.Gt(1))
                      //.Count().As("ccc")
                      .Select<string>("s").By("id")
                      //.Select<Vertex>("s").As("ss")
                      .Count()
                      //.Where(__.As("s").Count().Is(P.Gt(1)))
                      .ToList();
                     //.Select<Vertex>("cnt").Values<int>().ToList();

            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }

    }
}
