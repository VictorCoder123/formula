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
                    string idName = (id == null)? "_AUTOID_" + ft.GetHashCode() : id.Name; 
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
                    string idName = "_AUTOID_" + term.GetHashCode();
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
                            executor.connectCnstToType(value, false);
                        }
                        else if (((Cnst)obj).CnstKind == CnstKind.String && !cnstSet.Contains(((Cnst)obj).GetStringValue()))
                        {         
                            value = ((Cnst)obj).GetStringValue();
                            cnstSet.Add(value);
                            executor.AddCnstVertex(value, true);
                            executor.connectCnstToType(value, true);
                        }                      
                        executor.connectCnstToFuncTerm(idx, value, "ARG_" + i);
                    }
                }
            }
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
            
            // Map label to a list of tuples (type, index), type is the name of Function.
            Dictionary<String, List<Tuple<String, int>>> labelMap = new Dictionary<string, List<Tuple<string, int>>>();
            var bodies = ((Rule)rule.Node).Bodies;
            var traversal = executor.NewTraversal().V();

            foreach (Find find in bodies.ElementAt(0).Children)
            {
                FuncTerm ft = (FuncTerm)find.Match;
                string typeName = ((Id)ft.Function).Name;
                for (int i = 0; i < ft.Args.Count(); i++)
                {
                    Id id = (Id)ft.Args.ElementAt(i);
                    string label = id.Name;
                    if (!labelMap.ContainsKey(label))
                    {
                        List<Tuple<String, int>> list = new List<Tuple<string, int>>();
                        labelMap.Add(label, list);
                    }
                    List<Tuple<String, int>> tuples;
                    labelMap.TryGetValue(label, out tuples);
                    tuples.Add(new Tuple<String, int>(typeName, i));
                }
            }

            List<GraphTraversal<object, Vertex>> subTraversals = new List<GraphTraversal<object, Vertex>>();
            foreach (KeyValuePair<String, List<Tuple<String, int>>> entry in labelMap)
            {
                string label = entry.Key;
                foreach (Tuple<String, int> tuple in entry.Value)
                {
                    string type = tuple.Item1;
                    int index = tuple.Item2;
                    List<String> argList;
                    TypeArgsMap.TryGetValue(type, out argList);
                    string argType = argList.ElementAt(index);

                    var t1 = __.As(label).Out("type").Has("type", argType);
                    var t2 = __.As(label).Out("ARG_" + index).As(label + "_" + index);
                    var t3 = __.As(label + "_" + index).Out("type").Has("type", type);                
                    subTraversals.Add(t1);
                    subTraversals.Add(t2);
                    subTraversals.Add(t3);
                }
            }

            var steps = traversal.Match<Vertex>(subTraversals.ToArray()).Select<Vertex>("a");
            var vertices = steps.ToList();

            //Console.WriteLine(steps.Bytecode.StepInstructions);
        }

    }
}
