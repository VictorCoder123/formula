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

    using Microsoft.Formula.GraphDatabaseEngine.Domains;

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

        public List<Formula_Root.Argument> FormulaTermArgumentList { get; } = new List<Formula_Root.Argument>();
        public List<Formula_Root.Arguments> FormulaTermArgumentsList { get; } = new List<Formula_Root.Arguments>();

        public Dictionary<string, Formula_Root.BaseType> FormulaTermBaseTypeDict { get; } = new Dictionary<string, Formula_Root.BaseType>();

        public List<Formula_Root.UnionList> FormulaTermUnionListList { get; } = new List<Formula_Root.UnionList>();
        public Dictionary<string, Formula_Root.UnionType> FormulaTermUnionTypeDict { get; } = new Dictionary<string, Formula_Root.UnionType>();

        public List<Formula_Root.EnumList> FormulaTermEnumListList { get; } = new List<Formula_Root.EnumList>();
        public Dictionary<string, Formula_Root.EnumType> FormulaTermEnumTypeDict = new Dictionary<string, Formula_Root.EnumType>();

        public List<Formula_Root.Label> FormulaTermLabelList { get; } = new List<Formula_Root.Label>();
        public List<Formula_Root.Labels> FormulaTermLabelsList { get; } = new List<Formula_Root.Labels>();

        public List<Formula_Root.ModelExpr> FormulaTermModelExprList { get; } = new List<Formula_Root.ModelExpr>();
        public List<Formula_Root.BinaryExpr1> FormulaTermBinaryExpr1List { get; } = new List<Formula_Root.BinaryExpr1>();
        public List<Formula_Root.BinaryExpr2> FormulaTermBinaryExpr2List { get; } = new List<Formula_Root.BinaryExpr2>();
        public List<Formula_Root.BinaryExpr3> FormulaTermBinaryExpr3List { get; } = new List<Formula_Root.BinaryExpr3>();

        public List<Formula_Root.Head> FormulaTermHeadList { get; } = new List<Formula_Root.Head>();
        public List<Formula_Root.Body> FormulaTermBodyList { get; } = new List<Formula_Root.Body>();
        public List<Formula_Root.Rule> FormulaTermRuleList { get; } = new List<Formula_Root.Rule>();

        public IEnumerable<ICSharpTerm> Terms
        {
            get
            {
                foreach (Formula_Root.Argument ag in FormulaTermArgumentList)
                {
                    yield return ag;
                }

                foreach (Formula_Root.Arguments ags in FormulaTermArgumentsList)
                {
                    yield return ags;
                }

                foreach (Formula_Root.BaseType bt in FormulaTermBaseTypeDict.Values)
                {
                    yield return bt;
                }

                foreach (Formula_Root.UnionList ul in FormulaTermUnionListList)
                {
                    yield return ul;
                }

                foreach (Formula_Root.UnionType ut in FormulaTermUnionTypeDict.Values)
                {
                    yield return ut;
                }

                foreach (Formula_Root.Label lb in FormulaTermLabelList)
                {
                    yield return lb;
                }

                foreach (Formula_Root.Labels lbs in FormulaTermLabelsList)
                {
                    yield return lbs;
                }

                foreach (Formula_Root.ModelExpr me in FormulaTermModelExprList)
                {
                    yield return me;
                }

                foreach (Formula_Root.BinaryExpr1 be1 in FormulaTermBinaryExpr1List)
                {
                    yield return be1;
                }

                foreach (Formula_Root.BinaryExpr2 be2 in FormulaTermBinaryExpr2List)
                {
                    yield return be2;
                }

                foreach (Formula_Root.BinaryExpr3 be3 in FormulaTermBinaryExpr3List)
                {
                    yield return be3;
                }
            }
        }

        public void GenerateFormulaTerms()
        {
            foreach (string type in TypeArgsMap.Keys)
            {
                AddBaseType(type);
            }

            foreach (Rule rule in Rules)
            {
                Body body = rule.Bodies.ElementAt(0);
                Formula_Root.Body bodyTerm = AddBody(body);
            }
        }

        public Formula_Root.Rule AddRule()
        {

        }

        public Formula_Root.Head AddHead()
        {

        }

        public Formula_Root.Body AddBody(Body body)
        {
            var labelMap = new LabelMap(body, this);
            List<Formula_Root.ModelExpr> modelExprList = new List<Formula_Root.ModelExpr>(); 
            List<KeyValuePair<string, int>> typeInstanceIndexPairs = labelMap.GetAllTypeInstancePair();
            foreach (var pair in typeInstanceIndexPairs)
            {
                string type = pair.Key;
                int instanceIndex = pair.Value;
                List<string> labels = labelMap.GetLabelsInSingleExpr(type, instanceIndex);
                List<Formula_Root.Label> labelTerms = new List<Formula_Root.Label>();
                foreach (string label in labels)
                {
                    Formula_Root.Label labelTerm = Formula_Root.MkLabel(Formula_Root.MkString(label), AddBaseType(type));
                    labelTerms.Add(labelTerm);
                }
                Formula_Root.Labels FormulaTermLabelListTerm = AddLabels(labelTerms);
                Formula_Root.ModelExpr expr = AddModelExpr(AddBaseType(type), FormulaTermLabelListTerm);
                modelExprList.Add(expr);
                FormulaTermModelExprList.Add(expr);
            }

            int index = modelExprList.Count - 1;
            Formula_Root.Body bodyTerm = Formula_Root.MkBody(modelExprList.ElementAt(index), Formula_Root.MkNumeric(index), Formula_Root.MkUserCnst(Formula_Root.UserCnstKind.NIL));
            FormulaTermBodyList.Add(bodyTerm);

            for (int i = modelExprList.Count - 2; i >= 0; i--)
            {
                bodyTerm = Formula_Root.MkBody(modelExprList.ElementAt(i), Formula_Root.MkNumeric(i), bodyTerm);
                FormulaTermBodyList.Add(bodyTerm);
            }

            return bodyTerm;
        }

        public Formula_Root.Arguments AddArguments(string type)
        {
            if (TypeArgsMap.ContainsKey(type))
            {
                int index = TypeArgsMap[type].Count() - 1;
                Formula_Root.Argument lastArgument = AddArgument(type, index);
                Formula_Root.Arguments arguments = Formula_Root.MkArguments(lastArgument, Formula_Root.MkNumeric(index), Formula_Root.MkUserCnst(Formula_Root.UserCnstKind.NIL));
                FormulaTermArgumentsList.Add(arguments);
                for (int i = TypeArgsMap[type].Count() - 2; i >= 0; i--)
                {
                    Formula_Root.Argument argument = AddArgument(type, i);
                    arguments = Formula_Root.MkArguments(argument, Formula_Root.MkNumeric(i), arguments);
                    FormulaTermArgumentsList.Add(arguments);
                }
                return arguments;
            }
            return null;
        }

        public Formula_Root.Argument AddArgument(string type, int index)
        {
            string argTypeName = TypeArgsMap[type].ElementAt(index);
            string label = TypeArgsLabelMap[type].ElementAt(index);
            Formula_Root.BaseType argBaseType;
            if (FormulaTermBaseTypeDict.ContainsKey(argTypeName))
            {
                argBaseType = FormulaTermBaseTypeDict[argTypeName];
            }
            else
            {
                argBaseType = AddBaseType(argTypeName);
                if (!FormulaTermBaseTypeDict.ContainsKey(argTypeName))
                {
                    FormulaTermBaseTypeDict.Add(argTypeName, argBaseType);
                } 
            }
            Formula_Root.Argument argument = Formula_Root.MkArgument(Formula_Root.MkString(label), argBaseType);
            FormulaTermArgumentList.Add(argument);
            return argument;
        }

        public Formula_Root.BaseType AddBaseType(string type)
        {
            if (FormulaTermBaseTypeDict.ContainsKey(type))
            {
                return FormulaTermBaseTypeDict[type];
            }

            Formula_Root.BaseType baseType = null;
            if (type == "String" || type == "Integer" || type == "Boolean" || type == "Real" || type == "Natural")
            {
                baseType = Formula_Root.MkBaseType(Formula_Root.MkString(type), Formula_Root.MkUserCnst(Formula_Root.UserCnstKind.NIL));
                return baseType;
            }
            else if (UnionTypeMap.ContainsKey(type))
            {
                if (!FormulaTermUnionTypeDict.ContainsKey(type))
                {
                    Formula_Root.UnionType unionType = AddUnionType(type);
                }
            }
            else
            {
                Formula_Root.Arguments arguments = AddArguments(type);
                baseType = Formula_Root.MkBaseType(Formula_Root.MkString(type), arguments);
            }

            if (baseType != null && !FormulaTermBaseTypeDict.ContainsKey(type))
            {
                FormulaTermBaseTypeDict.Add(type, baseType);
            }
               
            return baseType;
        }

        // Return existing term or generate new term and return it.
        public object GetTermByType(string type)
        {
            string subtypeName = type;
            object subtypeTerm = null;
            if (UnionTypeMap.ContainsKey(subtypeName))
            {
                if (FormulaTermUnionTypeDict.ContainsKey(subtypeName))
                {
                    subtypeTerm = FormulaTermUnionTypeDict[subtypeName];
                }
                else
                {
                    subtypeTerm = AddUnionType(subtypeName);
                }
            }
            else
            {
                if (FormulaTermBaseTypeDict.ContainsKey(subtypeName))
                {
                    subtypeTerm = FormulaTermBaseTypeDict[subtypeName];
                }
                else
                {
                    subtypeTerm = AddBaseType(subtypeName);
                }
            }

            return subtypeTerm;
        }

        public Formula_Root.UnionList AddUnionList(string type)
        {
            int index = UnionTypeMap[type].Count - 1;
            string subtypeName = UnionTypeMap[type].ElementAt(index);
            object subtypeTerm = GetTermByType(subtypeName);

            Formula_Root.UnionList unionList = Formula_Root.MkUnionList(subtypeTerm as Formula_Root.IArgType_UnionList__0, Formula_Root.MkUserCnst(Formula_Root.UserCnstKind.NIL));
            for (int i = UnionTypeMap[type].Count - 2; i >= 0; i--)
            {
                subtypeName = UnionTypeMap[type].ElementAt(i);
                subtypeTerm = GetTermByType(subtypeName);
                unionList = Formula_Root.MkUnionList(subtypeTerm as Formula_Root.IArgType_UnionList__0, unionList);
            }
            return unionList;
        }

        public Formula_Root.UnionType AddUnionType(string type)
        {
            if (FormulaTermUnionTypeDict.ContainsKey(type))
            {
                return FormulaTermUnionTypeDict[type];
            }
            else
            {
                Formula_Root.UnionList unionList = AddUnionList(type);
                Formula_Root.UnionType utype = Formula_Root.MkUnionType(Formula_Root.MkString(type), unionList);
                FormulaTermUnionTypeDict.Add(type, utype);
                return utype;
            }
        }

        /*
        public Formula_Root.EnumList AddEnumList()
        {

        }

        public Formula_Root.EnumType AddEnumType()
        {

        }
        */

        public Formula_Root.Label AddLabel(string label, string type)
        {
            object term = GetTermByType(type);
            Formula_Root.Label labelTerm = Formula_Root.MkLabel(Formula_Root.MkString(label), term as Formula_Root.IArgType_Label__1, 
                Formula_Root.MkUserCnst(Formula_Root.UserCnstKind.NIL));
            FormulaTermLabelList.Add(labelTerm);
            return labelTerm;
        }

        public Formula_Root.Labels AddLabels(List<Formula_Root.Label> labels)
        {
            int lastIndex = labels.Count - 1;
            Formula_Root.Labels labelsTerm = Formula_Root.MkLabels(labels[lastIndex], Formula_Root.MkNumeric(lastIndex), Formula_Root.MkUserCnst(Formula_Root.UserCnstKind.NIL));
            for (int i = labels.Count-2; i >= 0; i--)
            {
                labelsTerm = Formula_Root.MkLabels(labels[i], Formula_Root.MkNumeric(i), labelsTerm);
            }
            FormulaTermLabelsList.Add(labelsTerm);
            return labelsTerm;
        }

        public Formula_Root.ModelExpr AddModelExpr(object typeTerm, Formula_Root.Labels labels)
        {
            Formula_Root.ModelExpr modelExpr = Formula_Root.MkModelExpr(typeTerm as Formula_Root.IArgType_ModelExpr__0, labels);
            FormulaTermModelExprList.Add(modelExpr);
            return modelExpr;
        }

        public Formula_Root.BinaryExpr1 AddBinaryExpr1(Formula_Root.UserCnst op, Formula_Root.Label left, Formula_Root.Label right)
        {
            Formula_Root.BinaryExpr1 be1 = Formula_Root.MkBinaryExpr1(op, left, right);
            FormulaTermBinaryExpr1List.Add(be1);
            return be1;
        }

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
            BooleanMap[id] = value;
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
