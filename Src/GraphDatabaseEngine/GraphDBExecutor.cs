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
        private string gremlinScript = "graph = TinkerGraph.open();" + System.Environment.NewLine;

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

        public string GetGremlinScript()
        {
            return gremlinScript;
        }

        public void WriteGremlinScriptToFile(string formulaFile)
        {
            string gremlinFile = "script.groovy";
            if (formulaFile != null)
            {
                gremlinFile = formulaFile + ".groovy";
            }
            System.IO.File.WriteAllText(gremlinFile, gremlinScript);
        }

        // TODO: Export existing database to a local file.
        public void ExportGraph()
        {
            //graph.Traversal().
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
            gremlinScript += string.Format("graph.traversal().addV().property('scope', '{0}').property('domain', '{1}');" + System.Environment.NewLine, domain, domain);
        }

        public void AddTypeVertex(string type, string domain)
        {
            AddVertex("meta", type, "domain", domain);
            gremlinScript += string.Format("graph.traversal().addV().property('meta', '{0}').property('domain', '{1}');" + System.Environment.NewLine, type, domain);
        }

        public void AddModelVertex(string id, string domain)
        {
            AddVertex("id", id, "domain", domain);
            gremlinScript += string.Format("graph.traversal().addV().property('id', '{0}').property('domain', '{1}');" + System.Environment.NewLine, id, domain);
        }

        public void AddCnstVertex(string valueString, bool isString, string domain)
        {
            var source = NewTraversal(graph).AddV();

            string boolValueStr = "false";
            if (isString)
            {
                boolValueStr = "true";
            }

            if (isString)
            {
                source = source.Property("value", valueString);
                gremlinScript += string.Format("graph.traversal().addV().property('value', '{0}').property('domain', '{1}').property('isString', {2});" + System.Environment.NewLine, valueString, domain, boolValueStr);
            }
            else
            {
                int num = ToInteger(valueString);
                source = source.Property("value", num);
                gremlinScript += string.Format("graph.traversal().addV().property('value', '{0}').property('domain', '{1}').property('isString', {2});" + System.Environment.NewLine, num, domain, boolValueStr);
            }

            source = source.Property("isString", isString);
            source = source.Property("domain", domain);
            source.ToList();
        }

        // Add two boolean nodes to graph database.
        public void AddBooleanVertexes(string domain)
        {
            AddVertex("bool", true, "domain", domain);
            AddVertex("bool", false, "domain", domain);
            gremlinScript += string.Format("graph.traversal().addV().property('bool', {0}).property('domain', '{1}');" + System.Environment.NewLine, true, domain);
            gremlinScript += string.Format("graph.traversal().addV().property('bool', {0}).property('domain', '{1}');" + System.Environment.NewLine, false, domain);
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

        // Connect FuncTerm to boolean node if argument is of boolean type.
        public void connectFuncTermToBoolean(string id, bool boolValue, string edgeType, string domain)
        {
            AddEdge("id", id, "bool", boolValue, edgeType, domain);
            string boolValueStr = "false";
            if (boolValue)
            {
                boolValueStr = "true";
            }
            gremlinScript += string.Format("graph.traversal().V().has('bool', {2}).has('domain', '{1}').as('a').V().has('id', '{0}').has('domain', '{3}').addE('{4}').to('a').toList();" + System.Environment.NewLine, 
                id, domain, boolValueStr, domain, edgeType);
        }

        // Connect Cnst to FuncTerm as argument. (FuncTerm -> cnst)
        public void connectFuncTermToCnst(string id, string cnstString, bool isString, string edgeType, string edgeLabel, string domain)
        {
            if (isString)
            {
                AddEdge("id", id, "value", cnstString, edgeType, domain);
                gremlinScript += string.Format("graph.traversal().V().has('value', '{2}').has('domain', '{1}').as('a').V().has('id', '{0}').has('domain', '{3}').addE('{4}').to('a').toList();" + System.Environment.NewLine,
                id, domain, cnstString, domain, edgeType);
                if (edgeLabel != null)
                {
                    AddEdge("id", id, "value", cnstString, edgeLabel, domain);
                    gremlinScript += string.Format("graph.traversal().V().has('value', '{2}').has('domain', '{1}').as('a').V().has('id', '{0}').has('domain', '{3}').addE('{4}').to('a').toList();" + System.Environment.NewLine,
                    id, domain, cnstString, domain, edgeLabel);
                }
            }
            else
            {
                int num = ToInteger(cnstString);
                AddEdge("id", id, "value", num, edgeType, domain);
                gremlinScript += string.Format("graph.traversal().V().has('value', {2}).has('domain', '{1}').as('a').V().has('id', '{0}').has('domain', '{3}').addE('{4}').to('a').toList();" + System.Environment.NewLine,
                id, domain, num, domain, edgeType);
                if (edgeLabel != null)
                {
                    AddEdge("id", id, "value", num, edgeLabel, domain);
                    gremlinScript += string.Format("graph.traversal().V().has('value', {2}).has('domain', '{1}').as('a').V().has('id', '{0}').has('domain', '{3}').addE('{4}').to('a').toList();" + System.Environment.NewLine,
                    id, domain, num, domain, edgeLabel);
                }
            }
        }

        // Connect Cnst to FuncTerm when numeric value is known.
        public void connectFuncTermToCnst(string id, int value, string edgeType, string edgeLabel, string domain)
        {
            AddEdge("id", id, "value", value, edgeType, domain);
            gremlinScript += string.Format("graph.traversal().V().has('value', {2}).has('domain', '{1}').as('a').V().has('id', '{0}').has('domain', '{3}').addE('{4}').to('a').toList();" + System.Environment.NewLine,
                id, domain, value.ToString(), domain, edgeType);
            if (edgeLabel != null)
            {
                AddEdge("id", id, "value", value, edgeLabel, domain);
                gremlinScript += string.Format("graph.traversal().V().has('value', {2}).has('domain', '{1}').as('a').V().has('id', '{0}').has('domain', '{3}').addE('{4}').to('a').toList();" + System.Environment.NewLine,
                id, domain, value.ToString(), domain, edgeLabel);
            }
        }

        // Connect type to its scope node. (type -> domain)
        public void connectTypeToDomain(string type, string domain)
        {
            string edgeLabel = "domain";
            AddEdge("meta", type, "scope", domain, edgeLabel, domain);
            gremlinScript += string.Format("graph.traversal().V().has('scope', '{0}').has('domain', '{0}').as('a').V().has('meta', '{1}').has('domain', '{0}').addE('{2}').to('a').toList();" + System.Environment.NewLine,
                domain, type, edgeLabel);
        }

        // Connect Cnst to enum type (cnst -> enum type)
        public void connectCnstToEnumType(string value, bool isString, string enumType, string domain)
        {
            if (isString)
            {
                AddEdge("value", value, "meta", enumType, "enum", domain);
                gremlinScript += string.Format("graph.traversal().V().has('meta', '{0}').has('domain', '{1}').as('a').V().has('value', '{2}').has('domain', '{3}').addE('{4}').to('a').toList();" + System.Environment.NewLine,
                    enumType, domain, value, domain, "enum");
            }
            else
            {
                int num = ToInteger(value);
                AddEdge("value", num, "meta", enumType, "enum", domain);
                gremlinScript += string.Format("graph.traversal().V().has('meta', '{0}').has('domain', '{1}').as('a').V().has('value', '{2}').has('domain', '{3}').addE('{4}').to('a').toList();" + System.Environment.NewLine,
                    enumType, domain, num, domain, "enum");
            }
        }

        // Connect two boolean nodes to their type node.
        public void connectBooleansToType(string domain)
        {
            AddEdge("bool", true, "meta", "Boolean", "type", domain);
            AddEdge("bool", false, "meta", "Boolean", "type", domain);
            gremlinScript += string.Format("graph.traversal().V().has('meta', '{0}').has('domain', '{1}').as('a').V().has('bool', {2}).has('domain', '{3}').addE('{4}').to('a').toList();" + System.Environment.NewLine,
                    "Boolean", domain, true, domain, "type");
            gremlinScript += string.Format("graph.traversal().V().has('meta', '{0}').has('domain', '{1}').as('a').V().has('bool', {2}).has('domain', '{3}').addE('{4}').to('a').toList();" + System.Environment.NewLine,
                    "Boolean", domain, false, domain, "type");
        }

        // Connect sub-type to its union type. (subtype -> type), the label name of edge is "type".
        public void connectSubtypeToType(string subtype, string type, string domain)
        {
            AddEdge("meta", subtype, "meta", type, "type", domain);
            gremlinScript += string.Format("graph.traversal().V().has('meta', '{0}').has('domain', '{1}').as('a').V().has('meta', '{2}').has('domain', '{3}').addE('{4}').to('a').toList();" + System.Environment.NewLine,
                    type, domain, subtype, domain, "type");
        }

        // Connect Cnst to its type node Integer or String. (cnst -> type)
        public void connectCnstToType(string value, bool isString, string domain)
        {
            if (isString)
            {
                AddEdge("value", value, "meta", "String", "type", domain);
                gremlinScript += string.Format("graph.traversal().V().has('meta', '{0}').has('domain', '{1}').as('a').V().has('value', '{2}').has('domain', '{3}').addE('{4}').to('a').toList();" + System.Environment.NewLine,
                    "String", domain, value, domain, "type");
            }
            else
            {
                int num = ToInteger(value);
                AddEdge("value", num, "meta", "Integer", "type", domain);
                gremlinScript += string.Format("graph.traversal().V().has('meta', '{0}').has('domain', '{1}').as('a').V().has('value', '{2}').has('domain', '{3}').addE('{4}').to('a').toList();" + System.Environment.NewLine,
                    "Integer", domain, num, domain, "type");
            }
        }

        // Connect type to argument type by auto-generated label (ARG_X) and user defined argument label. (type -> arg type)
        public void connectTypeToArgType(string type, string argType, string edgeType, string edgeLabel, string domain)
        {
            AddEdge("meta", type, "meta", argType, edgeType, domain);
            gremlinScript += string.Format("graph.traversal().V().has('meta', '{0}').has('domain', '{1}').as('a').V().has('meta', '{2}').has('domain', '{3}').addE('{4}').to('a').toList();" + System.Environment.NewLine,
                    argType, domain, type, domain, edgeType);
            if (edgeLabel != null)
            {
                AddEdge("meta", type, "meta", argType, edgeLabel, domain);
                gremlinScript += string.Format("graph.traversal().V().has('meta', '{0}').has('domain', '{1}').as('a').V().has('meta', '{2}').has('domain', '{3}').addE('{4}').to('a').toList();" + System.Environment.NewLine,
                    argType, domain, type, domain, edgeLabel);
            }
        }

        // Connect parent FuncTerm to argument FuncTerm. (idx -> idy)
        public void connectFuncTermToFuncTerm(string idx, string idy, string edgeType, string edgeLabel, string domain)
        {
            AddEdge("id", idx, "id", idy, edgeType, domain);
            gremlinScript += string.Format("graph.traversal().V().has('id', '{0}').has('domain', '{1}').as('a').V().has('id', '{2}').has('domain', '{3}').addE('{4}').to('a').toList();" + System.Environment.NewLine,
                    idy, domain, idx, domain, edgeType);
            if (edgeLabel != null)
            {
                AddEdge("id", idx, "id", idy, edgeLabel, domain);
                gremlinScript += string.Format("graph.traversal().V().has('id', '{0}').has('domain', '{1}').as('a').V().has('id', '{2}').has('domain', '{3}').addE('{4}').to('a').toList();" + System.Environment.NewLine,
                    idy, domain, idx, domain, edgeLabel);
            }
        }

        // Connect FuncTerm to type (id -> type)
        public void connectFuncTermToType(string id, string type, string domain)
        {
            AddEdge("id", id, "meta", type, "type", domain);
            gremlinScript += string.Format("graph.traversal().V().has('meta', '{0}').has('domain', '{1}').as('a').V().has('id', '{2}').has('domain', '{3}').addE('{4}').to('a').toList();" + System.Environment.NewLine,
                    type, domain, id, domain, "type");
        }

    }
}
