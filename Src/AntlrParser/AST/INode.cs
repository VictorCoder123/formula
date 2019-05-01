using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;

namespace Microsoft.Formula.AntlrParser.AST
{
    interface INode
    {
        ParserRuleContext Context { get; }
    }
}
