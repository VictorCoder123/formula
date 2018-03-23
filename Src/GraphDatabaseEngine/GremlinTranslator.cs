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
        public Dictionary<String, FuncTerm> IdMap { get; }
        public Dictionary<String, List<object>> FuncTermArgsMap { get; }
        public Dictionary<String, String> FuncTermTypeMap { get; }
        private HashSet<object> cnstSet = new HashSet<object>();

        public GremlinTranslator(string inputFile)
        {
            //GraphDBExecutor executor = new GraphDBExecutor("localhost", 8182);

            IdMap = new Dictionary<string, FuncTerm>();
            FuncTermArgsMap = new Dictionary<String, List<object>>();
            FuncTermTypeMap = new Dictionary<String, String>();

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
                    FuncTermTypeMap.Add(idName, (ft.Function as Id).Name);
                    TraverseFuncTerm(idName, ft);
                }
            );
        }

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

        public void TranslateQuery(GraphDBExecutor executor, string query)
        {
            var cmdLineName = new ProgramName("CommandLine.4ml");
            var parse = Factory.Instance.ParseText(
                cmdLineName,
                string.Format("domain Dummy {{q :-\n{0}\n.}}", query));
            parse.Wait();

            //WriteFlags(cmdLineName, parse.Result.Flags);
            if (!parse.Result.Succeeded)
            {
                Console.WriteLine("Failed to parse query.");
                //sink.WriteMessageLine("Could not parse goal", SeverityKind.Warning);
                return;
            }

            var rule = parse.Result.Program.FindAny(
                new API.ASTQueries.NodePred[]
                {
                    API.ASTQueries.NodePredFactory.Instance.Star,
                    API.ASTQueries.NodePredFactory.Instance.MkPredicate(NodeKind.Rule),
                });

            var bodies = ((Rule)rule.Node).Bodies;
            var traversal = executor.NewTraversal().V();
            foreach (Find find in bodies.ElementAt(0).Children)
            {
                FuncTerm ft = (FuncTerm)find.Match;
                string typeName = ((Id)ft.Function).Name;
                foreach (Id id in ft.Args)
                {

                }
                
                traversal.Match<Vertex>(
                    __.As("a").Has("type", ((Id)ft.Function).Name)
                );
            }

            
            
        }

        public void ExportToGraphDB(GraphDBExecutor executor)
        {
            // Insert all FuncTerm as vertex in GraphDB, // Unique ID -> Type Name 
            foreach (KeyValuePair<String, String> entry in FuncTermTypeMap)
            {
                executor.AddVertex(entry);
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
                        executor.AddEdgeToFuncTerm(idx, idy, "ARG_" + i);
                    }
                    else if (obj.GetType() == typeof(Cnst))
                    {
                        // Create node to store Const value.
                        List<KeyValuePair<String, object>> list = new List<KeyValuePair<string, object>>();

                        if (((Cnst)obj).CnstKind == CnstKind.Numeric && !cnstSet.Contains(((Cnst)obj).GetNumericValue()))
                        {
                            list.Add(new KeyValuePair<string, object>("type", "Integer"));
                            list.Add(new KeyValuePair<string, object>("value", ((Cnst)obj).GetNumericValue()));
                            cnstSet.Add(((Cnst)obj).GetNumericValue());
                        }
                        else if (((Cnst)obj).CnstKind == CnstKind.String && !cnstSet.Contains(((Cnst)obj).GetStringValue()))
                        {
                            list.Add(new KeyValuePair<string, object>("type", "String"));
                            list.Add(new KeyValuePair<string, object>("value", ((Cnst)obj).GetStringValue()));
                            cnstSet.Add(((Cnst)obj).GetStringValue());
                        }

                        if(list.Count() > 0) executor.AddVertex(list);
                        executor.AddEdgeToCnst(idx, obj, "ARG_" + i);
                    }
                }
            }
        }

    }
}
