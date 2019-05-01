using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime.Misc;


namespace AntlrParser
{  
    public class FormulaVisitor : FormulaBaseVisitor<int>
    {
        public override int VisitAtom([NotNull] FormulaParser.AtomContext context)
        {
            return base.VisitAtom(context);
        }
    }
}
