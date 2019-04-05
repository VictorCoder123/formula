namespace Microsoft.Formula.MuzEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Z3;

    class TestSuite
    {
        public void TestRelationFixpoint()
        {
            var settings = new Dictionary<String, String>();
            // settings.Add("engine", "datalog");
            Context context = new Context(settings);
            Solver solver = context.MkSolver();
            Fixedpoint fp = context.MkFixedpoint();

            var s = context.MkIntSort();
            var edge = context.MkFuncDecl("edge", new Sort[] { s, s }, context.BoolSort);
            var path = context.MkFuncDecl("path", new Sort[] { s, s }, context.BoolSort);

            var a = context.MkConst("a", s);
            var b = context.MkConst("b", s);
            var c = context.MkConst("c", s);

            fp.RegisterRelation(edge);
            fp.RegisterRelation(path);

            var edge_a_b = context.MkApp(edge, new Expr[] { a, b }) as BoolExpr;
            var path_a_b = context.MkApp(path, new Expr[] { a, b }) as BoolExpr;
            var path_a_c = context.MkApp(path, new Expr[] { a, c }) as BoolExpr;
            var path_b_c = context.MkApp(path, new Expr[] { b, c }) as BoolExpr;

            var edge_to_path = context.MkImplies(edge_a_b, path_a_b);
            var transite_closure = context.MkImplies(context.MkAnd(path_a_b, path_b_c), path_a_c);
            fp.AddRule(edge_to_path);
            fp.AddRule(transite_closure);

            var v1 = context.MkInt(1);
            var v2 = context.MkInt(2);
            var v3 = context.MkInt(3);
            var v4 = context.MkInt(4);

            fp.Query(new FuncDecl[] { edge });
            Console.WriteLine(fp.GetAnswer());

            solver.Add(context.MkDistinct(new Expr[] { a, b, c }));
            Console.WriteLine(solver.Check());
            Console.WriteLine(solver.Model);

            return;
        }

        public void TestDatatypes()
        {

        }

        public void TestMetaGraph()
        {

        }
    }
} 