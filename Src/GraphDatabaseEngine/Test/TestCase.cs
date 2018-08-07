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
            GremlinTranslator translator = new GremlinTranslator("modular_phone.4ml");
            translator.ExportAllDomainToGraphDB(executor);
            string query = "NodeInstanceOf(b,i)";
            Body body = translator.ParseQueryString(query);
            DomainStore store = translator.DomainStores["WebGME"];
            PrintBooleanMap(store);
            var queryResult = translator.GetQueryResult(executor, store, body, new List<string> { "b", "i" });
            return queryResult;
        }

        public List<Dictionary<string, object>> Test3(GraphDBExecutor executor)
        {
            GremlinTranslator translator = new GremlinTranslator("producer_consumer.4ml");
            translator.ExportAllDomainToGraphDB(executor);
            string query = "NodeInstanceOf(b,i)";
            Body body = translator.ParseQueryString(query);
            DomainStore store = translator.DomainStores["WebGME"];
            PrintBooleanMap(store);
            var queryResult = translator.GetQueryResult(executor, store, body, new List<string> { "b", "i" });
            return queryResult;
        }

        public List<Dictionary<string, object>> Test4(GraphDBExecutor executor)
        {
            GremlinTranslator translator = new GremlinTranslator("switchable_routes.4ml");
            translator.ExportAllDomainToGraphDB(executor);
            string query = "NodeInstanceOf(b,i)";
            Body body = translator.ParseQueryString(query);
            DomainStore store = translator.DomainStores["WebGME"];
            PrintBooleanMap(store);
            var queryResult = translator.GetQueryResult(executor, store, body, new List<string> { "b", "i" });
            return queryResult;
        }

        public List<Dictionary<string, object>> Test5(GraphDBExecutor executor)
        {
            GremlinTranslator translator = new GremlinTranslator("trackers_and_peers.4ml");
            translator.ExportAllDomainToGraphDB(executor);
            string query = "NodeInstanceOf(b,i)";
            Body body = translator.ParseQueryString(query);
            DomainStore store = translator.DomainStores["WebGME"];
            PrintBooleanMap(store);
            var queryResult = translator.GetQueryResult(executor, store, body, new List<string> { "b", "i" });
            return queryResult;
        }

        public List<Dictionary<string, object>> Test6(GraphDBExecutor executor)
        {
            GremlinTranslator translator = new GremlinTranslator("ten_machine_counter.4ml");
            translator.ExportAllDomainToGraphDB(executor);
            string query = "NodeInstanceOf(b,i)";
            Body body = translator.ParseQueryString(query);
            DomainStore store = translator.DomainStores["WebGME"];
            PrintBooleanMap(store);
            var queryResult = translator.GetQueryResult(executor, store, body, new List<string> { "b", "i" });
            return queryResult;
        }

        public List<Dictionary<string, object>> Test7(GraphDBExecutor executor)
        {
            GremlinTranslator translator = new GremlinTranslator("bip.4ml");
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
