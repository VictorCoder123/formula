namespace Microsoft.Formula.GraphDatabaseEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using API;
    using API.Nodes;
    using Common;
    using Common.Extras;
    using Common.Terms;

    public class FormulaTest
    {
        public static string[] COLORS = new string[] { "RED", "GREEN", "BLUE", "BLACK", "WHITE", "YELLOW" };
        public static string[] EDGETYPES = new string[] { "contains", "connects", "inherits" };
        private int vNum;
        private int eNum;
        private Env env;
        private List<Tuple<int, int>> randomPairs;

        public FormulaTest(int vNum, int eNum)
        {
            this.vNum = vNum;
            this.eNum = eNum;
            this.env = new Env();
            GraphGenerator gen = new GraphGenerator();
            randomPairs = gen.RandomEdges(vNum, eNum);
            string fileString = CreateRandomGraph(false);
            DoLoad(fileString);
        }

        public void DoLoad(string fileString)
        {
            ProgramName progName = ProgramName.emptyName;
            var task = Factory.Instance.ParseText(progName, fileString);
            task.Wait();

            InstallResult result;
            env.Install(task.Result.Program, out result);
            foreach (var kv in result.Touched)
            {
                Console.WriteLine(string.Format("({0}) {1}", kv.Status, kv.Program.Node.Name.ToString(env.Parameters)));
            }
            foreach (var f in result.Flags)
            {
                Console.WriteLine(
                    string.Format("{0} ({1}, {2}): {3}",
                    f.Item1.Node.Name.ToString(env.Parameters),
                    f.Item2.Span.StartLine,
                    f.Item2.Span.StartCol,
                    f.Item2.Message), f.Item2.Severity);
            }
        }

        public string CreateRandomGraph(bool print)
        {
            Random rnd = new Random();
            string modelString = @"
            ";
            for (int i=0; i<vNum; i++)
            {
                string color = COLORS[rnd.Next(0, COLORS.Length)];
                modelString += $@"  v{i} = V({i}, {color}).
                ";
            }

            for (int i=0; i<randomPairs.Count; i++)
            {
                int a = randomPairs.ElementAt(i).Item1;
                int b = randomPairs.ElementAt(i).Item2;
                modelString += $@"  e{i} = E(v{a}, v{b}).
                ";
            }

            String FormulaFileString =
                @"domain Graph 
                  {
                    V::= new (lbl: Integer, color: String).
                    E ::= new (src: V, dst: V).
                    path ::= (V, V).
                    path(a, b) :- E(a, b).
                    path(a, b) :- path(a, x), path(x, b).
                    conforms no path(u, u).
                  }

                  model g of Graph 
                  {" 
              + modelString
              + @"}

                transform Copy (GraphIn:: Graph) 
                returns (GraphOut:: Graph)
                {
                   GraphOut.V(x, color) :- GraphIn.V(x, color).
                   GraphOut.E(x, y) :- GraphIn.E(x, y).
                }

                transform RemoveSelfLoop (GraphIn:: Graph)
                returns (GraphOut:: Graph)
                {
                    GraphOut.V(x, color) :- GraphIn.V(x, color).
                    GraphOut.E(x, y) :- GraphIn.E(x, y), x != y.
                }";

            return FormulaFileString;
        }
    }
}
