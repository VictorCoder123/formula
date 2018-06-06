namespace Microsoft.Formula.GraphDatabaseEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Microsoft.Formula.API;
    using Microsoft.Formula.API.ASTQueries;
    using Microsoft.Formula.API.Nodes;
    using Microsoft.Formula.Common;
    using Microsoft.Formula.API.Generators;

    using Gremlin.Net.Structure;
    using Gremlin.Net.Process.Traversal;
    using Gremlin.Net.Driver;
    using Gremlin.Net.Driver.Remote;

    // TODO: Add more edges for Edge label like a1.x = 1
    // TODO: Distinguish integer from string in graph database
    // TODO: Negation
    // TODO: type constraints like  a is A
    // TODO: built-in operator like ==, !=, <=, >=, no
    // TODO: Handle transitive closure with Gremlin Loop Matching Pattern.
    public class GremlinTranslator
    {
        private static readonly char[] cmdSplitChars = new char[] { ' ' };

        public AST<Program> Program { get; }
        
        public Dictionary<string, DomainStore> DomainStores { get; }

        public GremlinTranslator(string inputFile)
        {
            DomainStores = new Dictionary<string, DomainStore>();

            Env env = new Env();
            InstallResult ires;
            ProgramName progName = new ProgramName(inputFile);
            env.Install(inputFile, out ires);

            if (!ires.Succeeded)
            {
                Console.WriteLine("System failed to install {0}.", inputFile);
            }

            foreach (var touched in ires.Touched)
            {
                if (touched.Program.Node.Name.Equals(progName))
                {
                    Program = touched.Program;
                    break;
                }
            }

            // Print out installation error messages.
            foreach (var kv in ires.Touched)
            {
                Console.WriteLine(string.Format("({0}) {1}", kv.Status, kv.Program.Node.Name.ToString(env.Parameters)));
            }

            foreach (var f in ires.Flags)
            {
                Console.WriteLine(
                    string.Format("{0} ({1}, {2}): {3}",
                    f.Item1.Node.Name.ToString(env.Parameters),
                    f.Item2.Span.StartLine,
                    f.Item2.Span.StartCol,
                    f.Item2.Message));
            }

            Program.FindAll(
                new NodePred[] { NodePredFactory.Instance.Star, NodePredFactory.Instance.MkPredicate(NodeKind.Domain) },
                (path, n) =>
                {
                    // Create separate store for each domain.
                    Domain domain = n as Domain;
                    DomainStore store = new DomainStore(domain.Name);

                    foreach (Rule rule in domain.Rules)
                    {
                        store.AddRule(rule);
                    }

                    foreach (var decl in domain.TypeDecls)
                    {
                        if (decl.NodeKind == NodeKind.ConDecl)
                        {
                            ConDecl conDecl = decl as ConDecl;
                            store.AddType(conDecl.Name);
                            foreach(Field field in conDecl.Fields)
                            {
                                // Add the label of each argument into store.
                                string argLabel = field.Name;
                                if (argLabel != null)
                                {
                                    store.AddTypeArgLabel(conDecl.Name, argLabel);
                                }
                                else
                                {
                                    store.AddTypeArgLabel(conDecl.Name, null);
                                }

                                if (field.Type.NodeKind == NodeKind.Id)
                                {
                                    string argType = ((Id)field.Type).Name;
                                    store.AddTypeArg(conDecl.Name, argType);
                                }
                                // Handle Union type which does not define a new type name. For example, A ::= new(id: Integer + String).
                                else if (field.Type.NodeKind == NodeKind.Union)
                                {
                                    Union union = field.Type as Union;
                                    string autoType = "AUTO_TYPE_" + field.Type.GetHashCode();
                                    store.AddTypeArg(conDecl.Name, autoType);
                                    HandleUnionType(union, autoType, store);
                                }
                            }
                        }
                        else if (decl.NodeKind == NodeKind.UnnDecl)
                        {
                            UnnDecl unnDecl = decl as UnnDecl;
                            string typename = unnDecl.Name;
                            Union union = unnDecl.Body as Union;
                            store.AddType(unnDecl.Name);
                            HandleUnionType(union, typename, store);
                        }
                    }

                    foreach (var composition in domain.Compositions)
                    {
                        if (composition.NodeKind == NodeKind.ModRef)
                        {
                            // Copy all domain info from inherited domain.
                            ModRef mr = composition;
                            string inheritedDomainName = mr.Name;
                            DomainStore inheritedDomainStore = DomainStores[inheritedDomainName];
                            store.CopyDomainStore(inheritedDomainStore);
                        }
                    }

                    // Each domain has its own store for all domain and model information.
                    DomainStores.Add(store.DomainName, store);
                }
            );

            // Find all models containing ModelFacts.
            Program.FindAll(
                new NodePred[] { NodePredFactory.Instance.Star, NodePredFactory.Instance.MkPredicate(NodeKind.Model) },
                (path, n) =>
                {
                    // Add model facts to its own domain store.
                    Model model = n as Model;
                    string domainName = model.Domain.Name;
                    DomainStore store;
                    DomainStores.TryGetValue(domainName, out store);

                    foreach (ModelFact mf in model.Facts)
                    {
                        FuncTerm ft = mf.Match as FuncTerm;
                        Id id = mf.Binding as Id;
                        string typeName = (ft.Function as Id).Name;
                        string idName = (id == null) ? "_AUTOID_" + ((Id)ft.Function).Name + ft.GetHashCode() : id.Name;
                        store.AddModel(idName, typeName);
                        
                        TraverseFuncTerm(idName, ft, store);
                    }
                }
            );
            
        }

        public void HandleUnionType(Union union, string typename, DomainStore store)
        {
            store.AddType(typename);
            // List<string> subTypes = new List<string>();
            foreach (var element in union.Children)
            {
                if (element.NodeKind == NodeKind.Id)
                {
                    //subTypes.Add((element as Id).Name);
                    store.AddUnionSubType(typename, (element as Id).Name);
                }
                else if (element.NodeKind == NodeKind.Enum)
                {
                    // Automatically generate a new union type for eumeration containing Cnst in string format.
                    string autoEnumType = "AUTO_ENUM_TYPE_" + element.GetHashCode();
                    store.AddUnionSubType(typename, autoEnumType);
                    store.AddType(autoEnumType);
                    
                    foreach (Cnst cnst in element.Children)
                    {
                        if (cnst.CnstKind == CnstKind.String)
                        {
                            store.AddUnionSubType(autoEnumType, cnst.GetStringValue());
                        }
                        else if (cnst.CnstKind == CnstKind.Numeric)
                        {
                            store.AddUnionSubType(autoEnumType, cnst.GetNumericValue().ToString());
                        }
                    }
                }
            }
        }

        // Check if a duplicate exists in all existing models of same type, args is a list of value for creating new model facts.
        public bool CheckDuplicate(string type, List<string> args, DomainStore store)
        {
            bool isDuplicate = false;
            // Find all FuncTerm with same type.
            List<string> ids = new List<string>();
            foreach (string id in store.GetAllModels())
            {
                if (store.GetModelType(id) == type) ids.Add(id);
            }

            List<string> argTypes = store.GetArgTypes(type); 
            foreach (string id in ids)
            {
                bool isSame = true;
                List<string> args2 = store.GetArgModels(id);  
                for (int i=0; i<argTypes.Count(); i++)
                {
                    if (argTypes[i] != "Integer" && argTypes[i] != "String")
                    {
                        if (!CheckFuncTermEquality(args[i], args2[i], store)) isSame = false;
                    }
                    else
                    {
                        if (args[i] != args2[i]) isSame = false;
                    }
                }
                if (isSame) return true;
            }
            return isDuplicate;
        }

        // Recursively check if the FuncTerm already exists.
        public bool CheckFuncTermEquality(string idx, string idy, DomainStore store)
        {
            bool isEqual = true;
            // FuncTerms belong to different types.
            if(store.GetModelType(idx) != store.GetModelType(idy))
            {
                return false;
            }
            else
            {
                string type = store.GetModelType(idx);
                List<string> argTypes = store.GetArgTypes(type);
                List<string> xargs = store.GetArgModels(idx);
                List<string> yargs = store.GetArgModels(idy);  
                for (int i=0; i<xargs.Count(); i++)
                {
                    if (argTypes[i] != "Integer" && argTypes[i] != "String")
                    {
                        if (!CheckFuncTermEquality(xargs[i], yargs[i], store))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (xargs[i] != yargs[i]) return false;
                    }
                }
            }
            return isEqual;
        }

        // Recursively find all FuncTerms inside FuncTerm and convert all values to string without any type information.
        private void TraverseFuncTerm(string id, FuncTerm ft, DomainStore store)
        {
            // Argument list only contains Ids of FuncTerm or converted string of Cnst (String or Numeric).
            for (int i = 0; i < ft.Args.Count(); i++)
            {
                if (ft.Args.ElementAt(i).NodeKind == NodeKind.Cnst)
                {
                    Cnst cnst = (Cnst)ft.Args.ElementAt(i);
                    if (cnst.CnstKind == CnstKind.Numeric)
                    {
                        store.AddModelArg(id, cnst.GetNumericValue().ToString());
                    }
                    else if(cnst.CnstKind == CnstKind.String)
                    {
                        store.AddModelArg(id, cnst.GetStringValue());
                    }
                }
                else if (ft.Args.ElementAt(i).NodeKind == NodeKind.Id)
                {
                    Id argId = (Id)ft.Args.ElementAt(i);
                    store.AddModelArg(id, argId.Name);
                }
                else if (ft.Args.ElementAt(i).NodeKind == NodeKind.FuncTerm)
                {
                    // Create Auto-generated Id for FuncTerm in arguments recursively.
                    FuncTerm term = (FuncTerm)ft.Args.ElementAt(i);
                    string type = ((Id)term.Function).Name;
                    string idName = "_AUTOID_" + type + term.GetHashCode();
                    TraverseFuncTerm(idName, term, store);
                    store.AddModelArg(id, idName);
                    store.AddModel(idName, (term.Function as Id).Name);
                }
            }
        }

        // Merge all results from disjoint groups together
        public List<Dictionary<string, string>> MergeResultGroups(List<List<Dictionary<string, string>>> resultGroups)
        {
            List<Dictionary<string, string>> finalResult = new List<Dictionary<string, string>>();
            foreach (List<Dictionary<string, string>> result in resultGroups)
            {
                if (finalResult.Count() == 0)
                {
                    finalResult = result;
                }
                else
                {
                    List<Dictionary<string, string>> newfinalResult = new List<Dictionary<string, string>>();
                    foreach (var dict1 in finalResult)
                    {
                        foreach (var dict2 in result)
                        {
                            var dict = dict1.Concat(dict2).ToDictionary(x => x.Key, x => x.Value); ;
                            newfinalResult.Add(dict);
                        }
                    }
                    finalResult = newfinalResult;
                }
            }
            return finalResult;
        }

        public void AddNonRecursiveModel(GraphDBExecutor executor, List<string> labels, DomainStore store, LabelMap labelMap, string funcName, List<Dictionary<string, string>> finalResult)
        {
            string domainName = store.DomainName;

            // Update new generated models into FuncTermArgsMap and IdTypeMap mappings
            foreach (Dictionary<string, string> dict in finalResult)
            {
                string idName = "_AUTOID_" + funcName + dict.GetHashCode();
                string typeName = funcName;

                // Get argument list from dictionary.
                List<string> args = new List<string>();
                for (int i = 0; i < labels.Count(); i++)
                {
                    string label = labels[i];
                    args.Add(dict[label]);
                }

                // Only add it to database when it is not a duplicate in existing models.
                if (!CheckDuplicate(typeName, args, store))
                {
                    store.AddModel(idName, funcName);

                    // Add a new model fact with unique ID.
                    executor.AddModelVertex(idName, domainName);
                    executor.AddProperty("id", idName, "type", typeName, domainName);
                    executor.connectFuncTermToType(idName, typeName, domainName);

                    for (int i = 0; i < labels.Count(); i++)
                    {
                        string label = labels[i];
                        string labelType = labelMap.GetLabelType(label);
                        if (labelType == "Integer")
                        {
                            string integerString = dict[label];
                            executor.connectFuncTermToCnst(idName, integerString, "ARG_" + i, domainName);
                        }
                        else if (labelType == "String")
                        {
                            string s = dict[label];
                            executor.connectFuncTermToCnst(idName, s, "ARG_" + i, domainName);
                        }
                        else
                        {
                            string idy = dict[label];
                            string argLabel = store.TypeArgsLabelMap[funcName].ElementAt(i);
                            executor.connectFuncTermToFuncTerm(idName, idy, "ARG_" + i, argLabel, domainName);
                        }
                    }

                    //FuncTermArgsMap.Add(idName, args);
                    foreach (string arg in args)
                    {
                        store.AddModelArg(idName, arg);
                    }
                }
            }           
        }

        // Return true only if all related labels does not exceed the constraints set in OperatorInfo list (label, operator, count)
        public bool SatisfyCountConstraint(LabelMap labelMap, int resultCount, HashSet<string> relatedLabels)
        {
            // Add count constraints of labels like count({s | ...}) > 1
            foreach (string relatedLabel in relatedLabels)
            {
                foreach (var op in labelMap.OperatorList)
                {
                    string label = op.Label;
                    Cnst cnst = op.Cnst;
                    int num = (int)cnst.GetNumericValue().Numerator;

                    if (relatedLabel == label)
                    {
                        if (op.Operator == RelKind.No)
                        {
                            if (resultCount > 0) return false;
                        }
                        else if (op.Operator == RelKind.Gt)
                        {
                            if (resultCount <= num) return false;
                        }
                        else if (op.Operator == RelKind.Lt)
                        {
                            if (resultCount >= num) return false;
                        }
                        else if (op.Operator == RelKind.Ge)
                        {
                            if (resultCount < num) return false;
                        }
                        else if (op.Operator == RelKind.Le)
                        {
                            if (resultCount > num) return false;
                        }
                        else if (op.Operator == RelKind.Eq)
                        {
                            if (resultCount != num) return false;
                        }
                        else // (op.Operator == RelKind.Neq)
                        {
                            if (resultCount == num) return false;
                        }
                    }
                }
            }

            return true;
            
        }

        public void ExecuteRule(Rule r, GraphDBExecutor executor, DomainStore store)
        {
            Body body = r.Bodies.ElementAt(0);
            var labelMap = new LabelMap(body, store);
            var allLabels = labelMap.GetAllLabels();
            List<HashSet<string>> groups = labelMap.GetSCCGroups(allLabels);
            List<List<Dictionary<string, string>>> resultGroups = new List<List<Dictionary<string, string>>>();

            foreach (HashSet<string> group in groups)
            {
                List<Dictionary<string, string>> result = GetQueryResult(executor, store, body, group.ToList());

                if (!SatisfyCountConstraint(labelMap, result.Count(), group))
                {   
                    // Terminate rule execution if the count of some labels does not satify constraints.
                    return;
                }

                resultGroups.Add(result);
            }

            List<Dictionary<string, string>> finalResult = MergeResultGroups(resultGroups);

            foreach (FuncTerm ft in r.Heads)
            {
                string funcName = (ft.Function as Id).Name;
                List<string> labels = new List<string>();
                foreach (var node in ft.Args)
                {
                    Id id = node as Id;
                    labels.Add(id.Name);
                }
                AddNonRecursiveModel(executor, labels, store, labelMap, funcName, finalResult);
            }
        }

        // Export all domain definition and models into Graph Database.
        public void ExportAllDomainToGraphDB(GraphDBExecutor executor)
        {
            foreach (DomainStore store in DomainStores.Values)
            {
                ExportOneDomainToGraphDB(executor, store);
            }
        }

        public void ExportOneDomainToGraphDB(GraphDBExecutor executor, DomainStore store)
        {
            string domainName = store.DomainName;
            executor.AddDomainVertex(domainName);

            // Insert all types as meta-level vertex in GraphDB.
            foreach (string type in store.typeSet)
            {
                executor.AddTypeVertex(type, domainName);
                // Connect type to its scope node.
                executor.connectTypeToDomain(type, domainName);
            }

            // Insert edge to denote the relation between union type and its subtypes or cnst and enum.
            foreach (KeyValuePair<String, List<string>> entry in store.UnionTypeMap)
            {
                string unionType = entry.Key;
                // Test if it is a union type or enum type
                string sample = entry.Value.ElementAt(0);
                if (store.typeSet.Contains(sample))
                {
                    foreach (string subtype in entry.Value)
                    {
                        executor.connectSubtypeToType(subtype, unionType, domainName);
                    }
                }
                else
                {
                    string enumType = unionType;
                    foreach (string cnstString in entry.Value)
                    {
                        // Determine the type of Cnst is either number or string.
                        Rational r;
                        bool isRational = Rational.TryParseDecimal(cnstString, out r);
                        if (isRational)
                        {
                            store.AddCnst(cnstString);
                            executor.AddCnstVertex(cnstString, false, domainName);
                            executor.AddProperty("value", cnstString, "type", "Integer", domainName);
                        }
                        else
                        {
                            store.AddCnst(cnstString);
                            executor.AddCnstVertex(cnstString, true, domainName);
                            executor.AddProperty("value", cnstString, "type", "String", domainName);
                        }
                    }
                }
            }

            // Insert all models(FuncTerm) to GraphDB and connect them to their type nodes.
            foreach (KeyValuePair<String, String> entry in store.IdTypeMap)
            {
                executor.AddModelVertex(entry.Key, domainName);
                string typeName = entry.Value;
                executor.AddProperty("id", entry.Key, "type", typeName, domainName);
                executor.connectFuncTermToType(entry.Key, typeName, domainName);
            }

            // Insert edge to denote the argument relationship between type and arg type
            // If argument label exists, add this label as an edge.
            foreach (KeyValuePair<string, List<string>> entry in store.TypeArgsMap)
            {
                string type = entry.Key;
                for (int i = 0; i < entry.Value.Count(); i++)
                {
                    List<string> list = store.TypeArgsLabelMap[type];
                    string argType = entry.Value.ElementAt(i);
                    string label = list.ElementAt(i);
                    executor.connectTypeToArgType(type, argType, "ARG_" + i, label, domainName);
                }
            }

            // Insert edge to denote the relation between FuncTerm and its arguments.
            foreach (KeyValuePair<String, List<string>> entry in store.FuncTermArgsMap)
            {
                string idx = entry.Key;
                for (int i = 0; i < entry.Value.Count(); i++)
                {
                    string obj = entry.Value[i];
                    string argType = store.GetArgTypeByIDIndex(idx, i);
                    // Integer is interpreted as string and may need some fixes later.
                    if (argType == "Integer")
                    {
                        string value = obj;
                        if (!store.cnstSet.Contains(value))
                        {
                            store.AddCnst(value);
                            executor.AddCnstVertex(value, false, domainName);
                            executor.AddProperty("value", value, "type", "Integer", domainName);
                            executor.connectCnstToType(value, false, domainName);
                        }
                        executor.connectFuncTermToCnst(idx, value, "ARG_" + i, domainName);
                    }
                    else if (argType == "String")
                    {
                        string value = obj;
                        if (!store.cnstSet.Contains(value))
                        {
                            store.AddCnst(value);
                            executor.AddCnstVertex(value, true, domainName);
                            executor.AddProperty("value", value, "type", "String", domainName);
                            executor.connectCnstToType(value, true, domainName);
                        }
                        executor.connectFuncTermToCnst(idx, value, "ARG_" + i, domainName);
                    }
                    else
                    {
                        string idy = obj;
                        string idxType = store.GetModelType(idx);
                        string label = store.TypeArgsLabelMap[idxType].ElementAt(i); // label can be null.
                        executor.connectFuncTermToFuncTerm(idx, idy, "ARG_" + i, label, domainName);
                    }
                }
            }

            // Execute rules defined in domain in sequence.
            // Execute rules defined in the domain to add more model facts into database.
            List<Rule> rules = store.Rules;

            // Execute rules in loop until no more new model fact is added into database.
            int oldModelCount = -1;
            int newModelCount = store.IdTypeMap.Count();
            while (newModelCount != oldModelCount)
            {
                oldModelCount = newModelCount;
                foreach(var rule in rules)
                {
                    ExecuteRule(rule, executor, store);
                }
                newModelCount = store.IdTypeMap.Count();
            }
        }

        public Body ParseQueryString(string query)
        {
            var cmdLineName = new ProgramName("CommandLine.4ml");
            var parse = Factory.Instance.ParseText(
                cmdLineName,
                string.Format("domain Dummy {{q :-\n{0}\n.}}", query));
            parse.Wait();

            // WriteFlags(cmdLineName, parse.Result.Flags);
            if (!parse.Result.Succeeded)
            {
                Console.WriteLine("Failed to parse query.");
                return null;
            }

            var rule = parse.Result.Program.FindAny(
                new API.ASTQueries.NodePred[]
                {
                    API.ASTQueries.NodePredFactory.Instance.Star,
                    API.ASTQueries.NodePredFactory.Instance.MkPredicate(NodeKind.Rule),
                });

            var bodies = ((Rule)rule.Node).Bodies;
            var body = bodies.ElementAt(0);
            return body;
        }      

        // outputLabels must be all related without disjoint labels.
        public List<Dictionary<string, string>> GetQueryResult(GraphDBExecutor executor, DomainStore store, Body body, List<string> outputLabels)
        {
            LabelMap labelMap = new LabelMap(body, store);
            string domainName = store.DomainName;

            // outputLabels should be a subset of all relatedLabels.
            // Take the first label and the rest should be included in relatedLabels.
            string firstLabel = outputLabels[0];
            var traversal = executor.NewTraversal().V();
            List<ITraversal> subTraversals = new List<ITraversal>();

            // Items in list are GraphTraversal<object, object> type.
            var relatedLabels = labelMap.FindSCCLabels(firstLabel);
            HashSet<String> labelSet = new HashSet<string>();
            HashSet<String> relatedBindingLabels = new HashSet<string>();

            foreach (string relatedLabel in relatedLabels)
            {
                if (!labelMap.IsBindingLabel(relatedLabel))
                {
                    List<LabelMap.LabelInfo> labelInfoList;
                    labelInfoList = labelMap.GetLabelOccuranceInfo(relatedLabel);

                    foreach (LabelMap.LabelInfo labelInfo in labelInfoList)
                    {
                        string type = labelInfo.Type;
                        int index = labelInfo.ArgIndex;
                        int count = labelInfo.InstanceIndex;
                        List<String> argList = store.GetArgTypes(type);
                        string argType = argList.ElementAt(index);

                        string instanceLabel = labelMap.GetBindingLabel(type, count);
                        if (instanceLabel == null)
                        {
                            instanceLabel = count + "_instance_of_" + type;
                        }
                        else
                        {
                            relatedBindingLabels.Add(instanceLabel);
                        }

                        var t1 = __.As(relatedLabel).In("ARG_" + index).Has("type", type).Has("domain", domainName).As(instanceLabel);
                        var t2 = __.As(instanceLabel).Has("type", type).Has("domain", domainName).Out("ARG_" + index).As(relatedLabel);

                        if (!labelSet.Contains(instanceLabel))
                        {
                            labelSet.Add(instanceLabel);
                        }

                        string commandString = string.Format(@"__.As({0}).In('ARG_{1}').Has('type', {2}).Has('domain', {5}).As('{4}');
__.As('{4}').Has('type', {2}).Has('domain', {5}).Out('ARG_{1}').As({0});", relatedLabel, index, type, count, instanceLabel, domainName);
                        Console.WriteLine(commandString);

                        subTraversals.Add(t1);
                        subTraversals.Add(t2);
                    }
                }
            }

            // Add label bindings like c1 is C(a, b) into outputLabels to be selected.
            foreach (string instanceLabel in relatedBindingLabels)
            {
                outputLabels.Add(instanceLabel);
            }          

            // Add constraints between related labels
            List<string> relatedLabelList = relatedLabels.ToList();
            for (int i = 0; i < relatedLabelList.Count(); i++)
            {
                string label1 = relatedLabelList[i];
                for (int j = i + 1; j < relatedLabelList.Count(); j++)
                {
                    string label2 = relatedLabelList[j];
                    var t = __.Where(label1, P.Neq(label2));
                    string commandString = string.Format(@"__.Where({0}, P.Neq({1});", label1, label2);
                    Console.WriteLine(commandString);
                    subTraversals.Add(t);
                }
            }

            // Make sure some intermediate labels of same type are not identical.
            List<string> diffLabels = labelSet.ToList();
            for (int i = 0; i < diffLabels.Count(); i++)
            {
                string difflabel = diffLabels[i];
                for (int j = i + 1; j < diffLabels.Count(); j++)
                {
                    string difflabel2 = diffLabels[j];
                    var t = __.Where(difflabel, P.Neq(difflabel2));
                    string commandString = string.Format(@"__.Where({0}, P.Neq({1});", difflabel, difflabel2);
                    Console.WriteLine(commandString);
                    subTraversals.Add(t);
                }
            }
           
            var matchResult = traversal.Match<Vertex>(subTraversals.ToArray());
            int labelCount = outputLabels.Count();
            IList<IDictionary<string, string>> list = new List<IDictionary<string, string>>();

            // Print out all selected labels in Select step
            string selectedLabelString = "Select(";
            for (int i=0; i < outputLabels.Count(); i++)
            {
                string label = outputLabels.ElementAt(i);
                if (i == outputLabels.Count - 1)
                {
                    selectedLabelString += "'" + label + "'";
                }
                else
                {
                    selectedLabelString += "'" + label + "', ";
                }
            }
            selectedLabelString += ")";
            Console.WriteLine(selectedLabelString);

            // Gremlin CSharp version does not provide Select<Vertex>(string[] keys) and have to use some tweaks.
            if (labelCount == 1)
            {
                string label = labelMap.GetLabelType(outputLabels[0]);
                string prop = (label == "String" || label == "Integer") ? "value" : "id";
                var stringList = matchResult.Select<string>(outputLabels[0]).By(prop).ToList();
                foreach (string str in stringList)
                {
                    var dict = new Dictionary<string, string>();
                    dict.Add(label, str);
                    list.Add(dict);
                }
            }
            else if (labelCount == 2)
            {
                string label1 = labelMap.GetLabelType(outputLabels[0]);
                string label2 = labelMap.GetLabelType(outputLabels[1]);
                string prop1 = (label1 == "String" || label1 == "Integer") ? "value" : "id";
                string prop2 = (label2 == "String" || label2 == "Integer") ? "value" : "id";
                list = matchResult.Select<string>(outputLabels[0], outputLabels[1]).By(prop1).By(prop2).ToList();
            }
            else if (labelCount > 2)
            {
                string label1 = labelMap.GetLabelType(outputLabels[0]);
                string label2 = labelMap.GetLabelType(outputLabels[1]);
                string prop1 = (label1 == "String" || label1 == "Integer") ? "value" : "id";
                string prop2 = (label2 == "String" || label2 == "Integer") ? "value" : "id";
                var middleResult = matchResult.Select<string>(outputLabels[0], outputLabels[1], outputLabels.GetRange(2, labelCount-2).ToArray()).By(prop1).By(prop2);
                for (int i = 2; i < labelCount; i++)
                {
                    string label = labelMap.GetLabelType(outputLabels[i]);
                    string prop = (label == "String" || label == "Integer") ? "value" : "id";
                    middleResult = middleResult.By(prop);
                }
                list = middleResult.ToList();
            }

            Console.WriteLine("\n");

            List<Dictionary<string, string>> convertedList = new List<Dictionary<string, string>>();
            foreach (var dict in list)
            {
                convertedList.Add((Dictionary<string, string>) dict);
            }

            return convertedList;
        }

    }
}
