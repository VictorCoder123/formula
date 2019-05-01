using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime.Misc;
using Microsoft.Formula.AntlrParser.AST;


namespace Microsoft.Formula.AntlrParser
{  
    public class ExprVisitor : FormulaBaseVisitor<Node>
    {
        public override Node VisitAtom([NotNull] FormulaParser.AtomContext context)
        {
      
            return base.VisitAtom(context);
        }

        public override Node VisitTypeDeclBody([NotNull] FormulaParser.TypeDeclBodyContext context)
        {
            Node unnBody = Visit(context.unnBody());
            return unnBody;
        }
    }
}
