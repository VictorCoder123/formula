namespace Microsoft.Formula.GraphDatabaseEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.Formula.API;
    using Microsoft.Formula.API.ASTQueries;
    using Microsoft.Formula.API.Nodes;
    using Microsoft.Formula.Common;
    using Microsoft.Formula.API.Generators;

    public class TestCase
    {
        public TestCase()
        {

        }

        public void PrintBooleanMap(DomainStore store)
        {
            Console.WriteLine("--------------Boolean Variable Map---------------");
            foreach (string key in store.BooleanMap.Keys)
            {
                bool boolean = store.BooleanMap[key];
                Console.WriteLine(String.Format("{0}: {1}", key, boolean.ToString()));
            }
            Console.WriteLine("-------------------------------------------------");
        }

        public List<Dictionary<string, object>> Test1(GraphDBExecutor executor)
        {
            GremlinTranslator translator = new GremlinTranslator("test.4ml");
            translator.ExportAllDomainToGraphDB(executor);
            string query = "C(a, b)";
            Body body = translator.ParseQueryString(query);
            DomainStore store = translator.DomainStores["TestModelPlus"];
            PrintBooleanMap(store);
            var queryResult = translator.GetQueryResult(executor, store, body, new List<string> { "a", "b" });
            return queryResult;
        }

        public List<Dictionary<string, object>> Test2(GraphDBExecutor executor)
        {
            GremlinTranslator translator = new GremlinTranslator("test2.4ml");
            translator.ExportAllDomainToGraphDB(executor);
            string query = "NodeInstanceOf(b,i)";
            Body body = translator.ParseQueryString(query);
            DomainStore store = translator.DomainStores["WebGME"];
            PrintBooleanMap(store);
            var queryResult = translator.GetQueryResult(executor, store, body, new List<string> { "b", "i" });
            return queryResult;
        }
    }
}
