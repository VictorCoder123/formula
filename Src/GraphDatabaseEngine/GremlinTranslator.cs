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

    // TODO: Distinguish integer from string in graph database
    // TODO: count{}
    // TODO: Union type
    // TODO: Negation
    // TODO: type constraints like  a is A
    // TODO: built-in operator like ==, !=, <=, >=, no
    // TODO: Handle transitive closure with Gremlin Loop Matching Pattern.
    public class GremlinTranslator
    {
        private static readonly char[] cmdSplitChars = new char[] { ' ' };

        public AST<Program> Program { get; }
        
        // Map Id of ModelFact to FuncTerm with original Id or auto-generated Id.
        public Dictionary<String, String> IdTypeMap { get; }

        // Map Id of ModelFact to a list of ModelFact Id or Constants in arguments.
        public Dictionary<String, List<string>> FuncTermArgsMap { get; }

        // Map type name to a list of type in arguments.
        public Dictionary<String, List<String>> TypeArgsMap { get; } 

        // Map Union type name to a list of its subtypes including Enum type.
        public Dictionary<String, List<String>> UnionTypeMap { get; }

        // A set of types defined in domain.
        public HashSet<String> typeSet = new HashSet<string>();

        // A set of Constants of either String or Integer type.
        private HashSet<string> cnstSet = new HashSet<string>();

        public GremlinTranslator(string inputFile)
        {          
            IdTypeMap = new Dictionary<string, string>();
            FuncTermArgsMap = new Dictionary<String, List<string>>();
            TypeArgsMap = new Dictionary<string, List<string>>();
            UnionTypeMap = new Dictionary<string, List<string>>();
            
            // Add built-in types Integer and String type.
            typeSet.Add("Integer");
            typeSet.Add("String");

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

            // Find all ConDecl in Domain.        
            Program.FindAll(
                new NodePred[] { NodePredFactory.Instance.Star, NodePredFactory.Instance.MkPredicate(NodeKind.ConDecl)},
                (path, n) =>
                {
                    ConDecl conDecl = n as ConDecl;
                    typeSet.Add(conDecl.Name);
                    List<String> argList = new List<string>();
                    for (int i=0; i<conDecl.Children.Count(); i++)
                    {
                        Field field = (Field)conDecl.Children.ElementAt(i);
                        if (field.Type.NodeKind == NodeKind.Id)
                        {
                            string argType = ((Id)field.Type).Name;
                            argList.Add(argType);
                        }
                        // Handle Union type which does not define a new type name. For example, A ::= new(id: Integer + String).
                        else if (field.Type.NodeKind == NodeKind.Union)
                        {
                            Union union = field.Type as Union;
                            string autoType = "AUTO_TYPE_" + field.Type.GetHashCode();
                            argList.Add(autoType);
                            HandleUnionType(union, autoType);
                        }                       
                    }
                    TypeArgsMap.Add(conDecl.Name, argList);
                }
            );

            // Find all UnionDecl in Domain
            Program.FindAll(
               new NodePred[] { NodePredFactory.Instance.Star, NodePredFactory.Instance.MkPredicate(NodeKind.UnnDecl)},
               (path, n) =>
               {
                   UnnDecl unnDecl = n as UnnDecl;
                   string typename = unnDecl.Name;
                   Union union = unnDecl.Body as Union;
                   typeSet.Add(unnDecl.Name);
                   HandleUnionType(union, typename);
               }
            );

            // Find all ModelFact in Program.
            Program.FindAll(
                new NodePred[] { NodePredFactory.Instance.Star, NodePredFactory.Instance.MkPredicate(NodeKind.ModelFact) },
                (path, n) =>
                {
                    ModelFact mf = n as ModelFact;
                    FuncTerm ft = mf.Match as FuncTerm;
                    Id id = mf.Binding as Id;
                    string idName = (id == null)? "_AUTOID_" + ((Id)ft.Function).Name + ft.GetHashCode() : id.Name; 
                    IdTypeMap.Add(idName, (ft.Function as Id).Name);
                    TraverseFuncTerm(idName, ft);
                }
            );
        }

        public void HandleUnionType(Union union, string typename)
        {
            typeSet.Add(typename);
            List<string> subTypes = new List<string>();
            foreach (var element in union.Children)
            {
                if (element.NodeKind == NodeKind.Id)
                {
                    subTypes.Add((element as Id).Name);
                }
                else if (element.NodeKind == NodeKind.Enum)
                {
                    // Automatically generate a new union type for eumeration containing Cnst in string format.
                    string autoEnumType = "AUTO_ENUM_TYPE_" + element.GetHashCode();
                    subTypes.Add(autoEnumType);
                    typeSet.Add(autoEnumType);
                    List<String> enumComponents = new List<string>();
                    foreach (Cnst cnst in element.Children)
                    {
                        if (cnst.CnstKind == CnstKind.String)
                        {
                            enumComponents.Add(cnst.GetStringValue());
                        }
                        else if (cnst.CnstKind == CnstKind.Numeric)
                        {
                            enumComponents.Add(cnst.GetNumericValue().ToString());
                        }
                    }
                    UnionTypeMap.Add(autoEnumType, enumComponents);
                }
            }
            UnionTypeMap.Add(typename, subTypes);
        }

        // Check if a duplicate exists in all existing models of same type.
        public bool CheckDuplicate(string type, List<string> args)
        {
            bool isDuplicate = false;
            // Find all FuncTerm with same type.
            List<string> ids = new List<string>();
            foreach (string id in IdTypeMap.Keys)
            {
                if (IdTypeMap[id] == type) ids.Add(id);
            }

            List<string> argTypes = TypeArgsMap[type];
            foreach (string id in ids)
            {
                bool isSame = true;
                List<string> args2 = FuncTermArgsMap[id];
                for (int i=0; i<argTypes.Count(); i++)
                {
                    if (argTypes[i] != "Integer" && argTypes[i] != "String")
                    {
                        if (!CheckFuncTermEquality(args[i], args2[i])) isSame = false;
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
        public bool CheckFuncTermEquality(string idx, string idy)
        {
            bool isEqual = true;
            // FuncTerms belong to different types.
            if (IdTypeMap[idx] != IdTypeMap[idy])
            {
                return false;
            }
            else
            {
                string type = IdTypeMap[idx];
                List<string> argTypes = TypeArgsMap[type];
                List<string> xargs = FuncTermArgsMap[idx];
                List<string> yargs = FuncTermArgsMap[idy];
                for (int i=0; i<xargs.Count(); i++)
                {
                    if (argTypes[i] != "Integer" && argTypes[i] != "String")
                    {
                        if (!CheckFuncTermEquality(xargs[i], yargs[i]))
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
        private void TraverseFuncTerm(string id, FuncTerm ft)
        {
            // Argument list only contains Ids of FuncTerm or converted string of Cnst (String or Numeric).
            List<string> args = new List<string>(); 
            for (int i = 0; i < ft.Args.Count(); i++)
            {
                if (ft.Args.ElementAt(i).NodeKind == NodeKind.Cnst)
                {
                    Cnst cnst = (Cnst)ft.Args.ElementAt(i);
                    if (cnst.CnstKind == CnstKind.Numeric)
                    {
                        args.Add(cnst.GetNumericValue().ToString());
                    }
                    else if(cnst.CnstKind == CnstKind.String)
                    {
                        args.Add(cnst.GetStringValue());
                    }
                }
                else if (ft.Args.ElementAt(i).NodeKind == NodeKind.Id)
                {
                    Id argId = (Id)ft.Args.ElementAt(i);
                    args.Add(argId.Name);
                }
                else if (ft.Args.ElementAt(i).NodeKind == NodeKind.FuncTerm)
                {
                    // Create Auto-generated Id for FuncTerm in arguments recursively.
                    FuncTerm term = (FuncTerm)ft.Args.ElementAt(i);
                    string type = ((Id)term.Function).Name;
                    string idName = "_AUTOID_" + type + term.GetHashCode();
                    TraverseFuncTerm(idName, term);
                    args.Add(idName);
                    IdTypeMap.Add(idName, (term.Function as Id).Name);
                }
            }

            FuncTermArgsMap.Add(id, args);
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

        public void AddNonRecursiveModel(GraphDBExecutor executor, List<string> labels, LabelMap labelMap, string funcName, List<Dictionary<string, string>> finalResult)
        {
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
                if (!CheckDuplicate(typeName, args))
                {
                    IdTypeMap.Add(idName, funcName);

                    // Add a new model fact with unique ID.
                    executor.AddModelVertex(idName);
                    executor.AddProperty("id", idName, "type", typeName);
                    executor.connectFuncTermToType(typeName, idName);

                    for (int i = 0; i < labels.Count(); i++)
                    {
                        string label = labels[i];
                        string labelType = labelMap.GetLabelType(label);
                        if (labelType == "Integer")
                        {
                            string integerString = dict[label];
                            executor.connectCnstToFuncTerm(idName, integerString, "ARG_" + i);
                        }
                        else if (labelType == "String")
                        {
                            string s = dict[label];
                            executor.connectCnstToFuncTerm(idName, s, "ARG_" + i);
                        }
                        else
                        {
                            string idy = dict[label];
                            executor.connectFuncTermToFuncTerm(idName, idy, "ARG_" + i);
                        }
                    }
                    FuncTermArgsMap.Add(idName, args);
                }
            }           
        }

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
                        if (op.Operator == RelKind.Gt)
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

        public void ExecuteRule(Rule r, GraphDBExecutor executor)
        {
            Body body = r.Bodies.ElementAt(0);
            var labelMap = new LabelMap(body, IdTypeMap, TypeArgsMap);
            var allLabels = labelMap.GetAllLabels();
            List<HashSet<string>> groups = labelMap.GetSCCGroups(allLabels);
            List<List<Dictionary<string, string>>> resultGroups = new List<List<Dictionary<string, string>>>();

            foreach (HashSet<string> group in groups)
            {
                List<Dictionary<string, string>> result = GetQueryResult(executor, body, group.ToList());

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
                AddNonRecursiveModel(executor, labels, labelMap, funcName, finalResult);
            }
        }

        // Get the type of argument corresponding to unique Id given Id and index.
        public string GetArgType(string id, int index)
        {
            string idType;
            IdTypeMap.TryGetValue(id, out idType);
            List<string> argTypes;
            TypeArgsMap.TryGetValue(idType, out argTypes);
            return argTypes[index];
        }

        public void ExportToGraphDB(GraphDBExecutor executor)
        {          
            // Insert all types as meta-level vertex in GraphDB.
            foreach (string type in typeSet)
            {
                executor.AddTypeVertex(type);
            }

            // Insert edge to denote the relation between union type and its subtypes or cnst and enum.
            foreach (KeyValuePair<String, List<string>> entry in UnionTypeMap)
            {
                string unionType = entry.Key;
                // Test if it is a union type or enum type
                string sample = entry.Value.ElementAt(0);
                if (typeSet.Contains(sample))
                {
                    foreach (string subtype in entry.Value)
                    {
                        executor.connectSubtypeToType(unionType, subtype);
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
                            cnstSet.Add(cnstString);
                            executor.AddCnstVertex(cnstString, false);
                            executor.AddProperty("value", cnstString, "type", "Integer");
                        }
                        else
                        {
                            cnstSet.Add(cnstString);
                            executor.AddCnstVertex(cnstString, true);
                            executor.AddProperty("value", cnstString, "type", "String");
                        }
                    }
                }
            }

            // Insert all models(FuncTerm) to GraphDB and connect them to their type nodes.
            foreach (KeyValuePair<String, String> entry in IdTypeMap)
            {
                executor.AddModelVertex(entry.Key);
                string typeName = entry.Value;
                executor.AddProperty("id", entry.Key, "type", typeName);
                executor.connectFuncTermToType(typeName, entry.Key);
            }

            // Insert edge to denote the relation between FuncTerm and its arguments.
            foreach (KeyValuePair<String, List<string>> entry in FuncTermArgsMap)
            {
                string idx = entry.Key;
                for (int i = 0; i < entry.Value.Count(); i++)
                {
                    string obj = entry.Value[i];
                    string argType = GetArgType(idx, i);
                    // Integer is interpreted as string and may need some fixes later.
                    if (argType == "Integer")
                    {
                        string value = obj;
                        if (!cnstSet.Contains(value))
                        {
                            cnstSet.Add(value);
                            executor.AddCnstVertex(value, false);
                            executor.AddProperty("value", value, "type", "Integer");
                            executor.connectCnstToType(value, false);
                        }
                        executor.connectCnstToFuncTerm(idx, value, "ARG_" + i);
                    }
                    else if (argType == "String")
                    {
                        string value = obj;
                        if (!cnstSet.Contains(value))
                        {
                            cnstSet.Add(value);
                            executor.AddCnstVertex(value, true);
                            executor.AddProperty("value", value, "type", "String");
                            executor.connectCnstToType(value, true);
                        }
                        executor.connectCnstToFuncTerm(idx, value, "ARG_" + i);
                    }
                    else
                    {
                        string idy = obj;
                        executor.connectFuncTermToFuncTerm(idx, idy, "ARG_" + i);
                    }
                }
            }

            // Execute rules defined in domain in sequence.
            // Execute rules defined in the domain to add more model facts into database.
            List<Rule> rules = new List<Rule>();
            Program.FindAll(
                new NodePred[] { NodePredFactory.Instance.Star, NodePredFactory.Instance.MkPredicate(NodeKind.Rule) },
                (path, n) =>
                {
                    Rule r = n as Rule;
                    rules.Add(r);
                }
            );

            // Execute rules in loop until no more new model fact is added into database.
            int oldModelCount = -1;
            int newModelCount = IdTypeMap.Count();
            while (newModelCount != oldModelCount)
            {
                oldModelCount = newModelCount;
                foreach(var rule in rules)
                {
                    ExecuteRule(rule, executor);
                }
                newModelCount = IdTypeMap.Count();
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
        public List<Dictionary<string, string>> GetQueryResult(GraphDBExecutor executor, Body body, List<string> outputLabels)
        {
            LabelMap labelMap = new LabelMap(body, IdTypeMap, TypeArgsMap);

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
                List<LabelMap.LabelInfo> labelInfoList;
                labelInfoList = labelMap.GetLabelOccuranceInfo(relatedLabel);

                foreach (LabelMap.LabelInfo labelInfo in labelInfoList)
                {
                    string type = labelInfo.Type;
                    int index = labelInfo.ArgIndex;
                    int count = labelInfo.InstanceIndex;
                    List<String> argList;
                    TypeArgsMap.TryGetValue(type, out argList);
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

                    var t1 = __.As(relatedLabel).Out("ARG_" + index).Has("type", type).As(instanceLabel);
                    var t2 = __.As(instanceLabel).Has("type", type).In("ARG_" + index).As(relatedLabel);

                    if (!labelSet.Contains(instanceLabel))
                    {
                        labelSet.Add(instanceLabel);
                    }

                    string commandString = string.Format(@"__.As({0}).Out('ARG_{1}').Has('type', {2}).As('{4}');
__.As('{4}').Has('type', {2}).In('ARG_{1}').As({0});", relatedLabel, index, type, count, instanceLabel);
                    Console.WriteLine(commandString);

                    subTraversals.Add(t1);
                    subTraversals.Add(t2);
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
