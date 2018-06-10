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

    public class DomainStore
    {
        // The name of domain
        public string DomainName { get; }

        // A list of its own rules and inherited rules.
        public List<Rule> Rules { get; }

        // Map Id of ModelFact to FuncTerm with original Id or auto-generated Id.
        public Dictionary<String, String> IdTypeMap { get; }

        // Map Id of ModelFact to a list of ModelFact Id or Constants in arguments.
        public Dictionary<String, List<string>> FuncTermArgsMap { get; }

        // Map type name to a list of type in arguments.
        public Dictionary<String, List<String>> TypeArgsMap { get; }

        public Dictionary<String, List<String>> TypeArgsLabelMap { get; }

        // Map Union type name to a list of its subtypes including Enum type.
        public Dictionary<String, List<String>> UnionTypeMap { get; }

        // A set of types defined in domain.
        public HashSet<String> typeSet { get; }

        // A set of Constants of either String or Integer type.
        public HashSet<string> cnstSet { get; }

        public Dictionary<String, Boolean> BooleanMap { get; }

        public DomainStore(string domainName)
        {
            DomainName = domainName;
            Rules = new List<Rule>();
            IdTypeMap = new Dictionary<string, string>();
            FuncTermArgsMap = new Dictionary<String, List<string>>();
            TypeArgsMap = new Dictionary<string, List<string>>();
            TypeArgsLabelMap = new Dictionary<string, List<string>>();
            UnionTypeMap = new Dictionary<string, List<string>>();
            typeSet = new HashSet<string>();
            cnstSet = new HashSet<string>();
            BooleanMap = new Dictionary<string, bool>();

            // Add built-in types Integer and String type.
            AddType("Integer");
            AddType("String");
            AddType("Boolean");
            AddType("Natural");
            AddType("Real");
        }

        // Copy domain information to domain store with ModRef.
        public void CopyDomainStore(DomainStore store)
        {
            foreach (var rule in store.Rules)
            {
                Rules.Add(rule);
            }

            foreach (var type in store.typeSet)
            {
                typeSet.Add(type);
            }

            foreach (var cnst in store.cnstSet)
            {
                cnstSet.Add(cnst);
            }

            foreach (var key in store.UnionTypeMap.Keys)
            {
                List<string> list = new List<string>(store.UnionTypeMap[key]);
                UnionTypeMap.Add(key, list);
            }

            foreach (var key in store.TypeArgsMap.Keys)
            {
                List<string> list = new List<string>(store.TypeArgsMap[key]);
                TypeArgsMap.Add(key, list);
            }

            foreach (var key in store.TypeArgsLabelMap.Keys)
            {
                List<string> list = new List<string>(store.TypeArgsLabelMap[key]);
                TypeArgsLabelMap.Add(key, list);
            }
        }

        public List<string> GetAllModels()
        {
            List<string> list = new List<string>();
            foreach (string id in IdTypeMap.Keys)
            {
                list.Add(id);
            }
            return list;
        }

        public string GetModelType(string id)
        {
            string type;
            IdTypeMap.TryGetValue(id, out type);
            return type;
        }

        public List<string> GetArgTypes(string type)
        {
            List<string> list;
            TypeArgsMap.TryGetValue(type, out list);
            return list;
        }

        public List<string> GetArgModels(string id)
        {
            List<string> list;
            FuncTermArgsMap.TryGetValue(id, out list);
            return list;
        }

        // Get the type of argument given its parent id and argument index.
        public string GetArgTypeByIDIndex(string id, int index)
        {
            string idType;
            IdTypeMap.TryGetValue(id, out idType);
            List<string> argTypes;
            TypeArgsMap.TryGetValue(idType, out argTypes);
            return argTypes[index];
        }

        public void AddRule(Rule rule)
        {
            Rules.Add(rule);
        }

        public void AddBooleanVariable(string id, bool value)
        {
            BooleanMap.Add(id, value);
        }

        public void AddModel(string id, string type)
        {
            if (!IdTypeMap.ContainsKey(id))
            {
                IdTypeMap.Add(id, type);
            }
        }

        public void AddType(string type)
        {
            if (!typeSet.Contains(type))
            {
                typeSet.Add(type);
            }
        }

        public void AddUnionSubType(string type, string subType)
        {
            if (!UnionTypeMap.ContainsKey(type))
            {
                List<string> list = new List<string>();
                list.Add(subType);
                UnionTypeMap.Add(type, list);
            }
            else
            {
                List<string> list;
                UnionTypeMap.TryGetValue(type, out list);
                list.Add(subType);
            }
        }

        public void AddTypeArg(string type, string arg)
        {
            if (!TypeArgsMap.ContainsKey(type))
            {
                List<string> list = new List<string>();
                list.Add(arg);
                TypeArgsMap.Add(type, list);
            }
            else
            {
                List<string> list;
                TypeArgsMap.TryGetValue(type, out list);
                list.Add(arg);
            }
        }

        // Add the label of argument into map.
        public void AddTypeArgLabel(string type, string label)
        {
            if (!TypeArgsLabelMap.ContainsKey(type))
            {
                List<string> list = new List<string>();
                list.Add(label);
                TypeArgsLabelMap.Add(type, list);
            }
            else
            {
                List<string> list;
                TypeArgsLabelMap.TryGetValue(type, out list);
                list.Add(label);
            }
        }

        public void AddModelArg(string id, string argId)
        {
            if (!FuncTermArgsMap.ContainsKey(id))
            {
                List<string> list = new List<string>();
                list.Add(argId);
                FuncTermArgsMap.Add(id, list);
            }
            else
            {
                List<string> list;
                FuncTermArgsMap.TryGetValue(id, out list);
                list.Add(argId);
            }
        }

        public void AddCnst(string cnst)
        {
            if (!cnstSet.Contains(cnst))
            {
                cnstSet.Add(cnst);
            }
        }
    }
}
