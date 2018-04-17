namespace Microsoft.Formula.GraphDatabaseEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Microsoft.Formula.API;
    using Microsoft.Formula.API.ASTQueries;
    using Microsoft.Formula.API.Nodes;
    using Microsoft.Formula.API.Generators;

    using Gremlin.Net.Structure;
    using Gremlin.Net.Process.Traversal;
    using Gremlin.Net.Driver;
    using Gremlin.Net.Driver.Remote;

    public class GremlinTranslator
    {
        private static readonly char[] cmdSplitChars = new char[] { ' ' };

        public AST<Program> Program { get; }
        
        // Map Id of ModelFact to FuncTerm with original Id or auto-generated Id.
        public Dictionary<String, FuncTerm> IdMap { get; }

        // Map Id of ModelFact to a list of ModelFact Id or Constants in arguments.
        public Dictionary<String, List<object>> FuncTermArgsMap { get; }

        // Map type name to a list of type in arguments.
        public Dictionary<String, List<String>> TypeArgsMap { get; } 

        // A set of types defined in domain.
        public HashSet<String> typeSet = new HashSet<string>();

        // A set of Constants of either String or Integer type.
        private HashSet<object> cnstSet = new HashSet<object>();

        public GremlinTranslator(string inputFile)
        {
            //GraphDBExecutor executor = new GraphDBExecutor("localhost", 8182);

            IdMap = new Dictionary<string, FuncTerm>();
            FuncTermArgsMap = new Dictionary<String, List<object>>();
            TypeArgsMap = new Dictionary<string, List<string>>();
            
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

            Program.FindAll(new NodePred[] { NodePredFactory.Instance.Star, NodePredFactory.Instance.MkPredicate(NodeKind.Domain)},
                (path, n) =>
                {

                }
            );

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
                        string argType = ((Id)field.Type).Name;
                        argList.Add(argType);
                    }
                    TypeArgsMap.Add(conDecl.Name, argList);
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
                    IdMap.Add(idName, ft);
                    TraverseFuncTerm(idName, ft);
                }
            );
        }

        // Recursively find all FuncTerms inside FuncTerm.
        private void TraverseFuncTerm(string id, FuncTerm ft)
        {
            // Argument list only contains Ids of FuncTerm or Cnst (String or Numeric).
            List<object> args = new List<object>(); 
            for (int i = 0; i < ft.Args.Count(); i++)
            {
                if (ft.Args.ElementAt(i).NodeKind == NodeKind.Cnst)
                {
                    Cnst cnst = (Cnst)ft.Args.ElementAt(i);
                    args.Add(cnst);
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
                    IdMap.Add(idName, term);
                }
            }

            FuncTermArgsMap.Add(id, args);
        }

        public void ExportToGraphDB(GraphDBExecutor executor)
        {
            // Insert all types as meta-level vertex in GraphDB.
            foreach (string type in typeSet)
            {
                executor.AddTypeVertex(type);
            }

            // Insert all models(FuncTerm) to GraphDB and connect them to their type nodes.
            foreach (KeyValuePair<String, FuncTerm> entry in IdMap)
            {
                executor.AddModelVertex(entry.Key);
                string typeName = ((Id)entry.Value.Function).Name;
                executor.AddProperty("id", entry.Key, "type", typeName);
                executor.connectFuncTermToType(typeName, entry.Key);
            }

            // Insert edge to denote the relation between FuncTerm and its arguments.
            foreach (KeyValuePair<String, List<object>> entry in FuncTermArgsMap)
            {
                string idx = entry.Key;
                for (int i = 0; i < entry.Value.Count(); i++)
                {
                    object obj = entry.Value.ElementAt(i);
                    if (obj.GetType() == typeof(String))
                    {
                        string idy = (String)obj;
                        executor.connectFuncTermToFuncTerm(idx, idy, "ARG_" + i);
                    }
                    else if (obj.GetType() == typeof(Cnst))
                    {
                        // TODO: Distinguish integer and string as they are both converted to string from Rational type.
                       
                        // Create node to store Const value and connect Cnst node to Cnst type node.   
                        string value = "";
                        if (((Cnst)obj).CnstKind == CnstKind.Numeric && !cnstSet.Contains(((Cnst)obj).GetNumericValue().ToString()))
                        {
                            value = ((Cnst)obj).GetNumericValue().ToString();
                            cnstSet.Add(value);
                            executor.AddCnstVertex(value, false);
                            executor.AddProperty("value", value, "type", "Integer");
                            executor.connectCnstToType(value, false);
                        }
                        else if (((Cnst)obj).CnstKind == CnstKind.String && !cnstSet.Contains(((Cnst)obj).GetStringValue()))
                        {         
                            value = ((Cnst)obj).GetStringValue();
                            cnstSet.Add(value);
                            executor.AddCnstVertex(value, true);
                            executor.AddProperty("value", value, "type", "String");
                            executor.connectCnstToType(value, true);
                        }                      
                        executor.connectCnstToFuncTerm(idx, value, "ARG_" + i);
                    }
                }
            }
        }

        // Overload and return a list of labels that are all strongly connected.
        public HashSet<String> FindSCCLabels(string label, Dictionary<string, List<Tuple<string, int, int>>> labelMap)
        {
            HashSet<String> relatedLabels = new HashSet<string>();
            FindSCCLabels(relatedLabels, label, labelMap);
            return relatedLabels;
        }

        // Recursively return a list of labels that are all strongly connected.
        public HashSet<String> FindSCCLabels(HashSet<String> labels, string label, Dictionary<string, List<Tuple<string, int, int>>> labelMap)
        { 
            labels.Add(label);

            List<Tuple<string, int, int>> srcTuples;
            labelMap.TryGetValue(label, out srcTuples);

            // srcTuples represents all occurance of target label.
            foreach (var srcTuple in srcTuples)
            {
                string typeName = srcTuple.Item1;
                int count = srcTuple.Item3;

                // Traverse all labels to find matches for target label.
                foreach (var dstLabel in labelMap.Keys)
                {
                    if (!labels.Contains(dstLabel))
                    {
                        List<Tuple<string, int, int>> dstTuples;
                        labelMap.TryGetValue(dstLabel, out dstTuples);
                        foreach (var dstTuple in dstTuples)
                        {
                            if (dstTuple.Item1 == typeName && dstTuple.Item3 == count)
                            {
                                labels.Add(dstLabel);
                                FindSCCLabels(labels, dstLabel, labelMap);
                            }
                        }
                    }
                }
            }

            return labels;
        }

        public void TranslateQuery(GraphDBExecutor executor, string query)
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
                return;
            }

            var rule = parse.Result.Program.FindAny(
                new API.ASTQueries.NodePred[]
                {
                    API.ASTQueries.NodePredFactory.Instance.Star,
                    API.ASTQueries.NodePredFactory.Instance.MkPredicate(NodeKind.Rule),
                });
            
            // Map label to a list of tuples (type, index, count), type is the name of Function that contains label.
            Dictionary<String, List<Tuple<String, int, int>>> labelMap = new Dictionary<string, List<Tuple<string, int, int>>>();
            Dictionary<String, int> typeCounts = new Dictionary<string, int>();

            var bodies = ((Rule)rule.Node).Bodies;

            foreach (Find find in bodies.ElementAt(0).Children)
            {
                FuncTerm ft = (FuncTerm)find.Match;
                string typeName = ((Id)ft.Function).Name;
                
                // Count the occurance of same type name in query.
                if (!typeCounts.ContainsKey(typeName))
                {
                    typeCounts.Add(typeName, 0);
                }
                else
                {
                    int oldCount;
                    typeCounts.TryGetValue(typeName, out oldCount);
                    typeCounts[typeName] = oldCount + 1;
                }

                int currentCount;
                typeCounts.TryGetValue(typeName, out currentCount);

                for (int i = 0; i < ft.Args.Count(); i++)
                {
                    Id id = (Id)ft.Args.ElementAt(i);
                    string label = id.Name;
                    if (!labelMap.ContainsKey(label))
                    {
                        List<Tuple<String, int, int>> list = new List<Tuple<string, int, int>>();
                        labelMap.Add(label, list);
                    }
                    List<Tuple<String, int, int>> tuples;
                    labelMap.TryGetValue(label, out tuples);
                    tuples.Add(new Tuple<String, int, int>(typeName, i, currentCount));
                }
            }

            //foreach (KeyValuePair<String, List<Tuple<String, int, int>>> entry in labelMap)
            foreach(var label in labelMap.Keys)
            {
                var traversal = executor.NewTraversal().V();
                List<ITraversal> subTraversals = new List<ITraversal>();
                // Items in list are GraphTraversal<object, object> type.
                var relatedLabels = FindSCCLabels(label, labelMap);
                HashSet<String> labelSet = new HashSet<string>();

                foreach (string relatedLabel in relatedLabels)
                {
                    List<Tuple<String, int, int>> tuples;
                    labelMap.TryGetValue(relatedLabel, out tuples);

                    foreach (Tuple<String, int, int> tuple in tuples)
                    {
                        string type = tuple.Item1;
                        int index = tuple.Item2;
                        int count = tuple.Item3;
                        List<String> argList;
                        TypeArgsMap.TryGetValue(type, out argList);
                        string argType = argList.ElementAt(index);
  
                        var t1 = __.As(relatedLabel).Out("ARG_" + index).Has("type", type).As(count + "_instance_of_" + type);
                        var t2 = __.As(count + "_instance_of_" + type).Has("type", type).In("ARG_" + index).As(relatedLabel);

                        if (!labelSet.Contains(count + "_instance_of_" + type))
                        {
                            labelSet.Add(count + "_instance_of_" + type);
                        }

                        // Print out the Gremlin query for debugging.
                        Console.WriteLine("__.As(\"" + relatedLabel + "\").Out(\"ARG_" + index + "\").Has(\"type\", \"" + type + "\").As(\"" + count + "_instance_of_" + type + "\"),");
                        Console.WriteLine("__.As(\"" + count + "_instance_of_" + type + "\").Has(\"type\", \"" + type + "\").In(\"ARG_" + index + "\").As(\"" + relatedLabel + "\"),");

                        subTraversals.Add(t1);
                        subTraversals.Add(t2);
                    }
                }

                // Make sure some intermediate labels of same type are not identical.
                List<string> diffLabels = labelSet.ToList();
                for (int i = 0; i < diffLabels.Count(); i++)
                {
                    string difflabel = diffLabels[i];
                    for (int j=i+1; j<diffLabels.Count(); j++)
                    {
                        string difflabel2 = diffLabels[j];
                        var t = __.Where(difflabel, P.Neq(difflabel2));
                        subTraversals.Add(t);
                    }
                }

                // Find the type of the label. Check if it is basic built-in type or other types.
                string propName;
                List<Tuple<string, int, int>> tupleList;
                labelMap.TryGetValue(label, out tupleList);
                string labelFuncType = tupleList[0].Item1;
                int labelIndex = tupleList[0].Item2;
                List<String> argTypeList;
                TypeArgsMap.TryGetValue(labelFuncType, out argTypeList);
                string labelType = argTypeList[labelIndex];

                if (labelType == "Integer" || labelType == "String")
                {
                    propName = "value";
                }
                else
                {
                    propName = "id";
                }

                var vertices = traversal.Match<Vertex>(subTraversals.ToArray()).Select<Vertex>(label).Values<String>(propName).ToList();
                foreach (string vid in vertices)
                {
                    Console.WriteLine(vid);
                }

                Console.WriteLine();
            }      

            //Console.WriteLine(steps.Bytecode.StepInstructions);
        }

    }
}
