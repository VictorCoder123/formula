using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

using Antlr4.Runtime;

namespace Microsoft.Formula.AntlrParser.AST
{

    public sealed class ModelFact : Node
    {
        public override int ChildCount
        {
            // get { return (Binding == null ? 1 : 2) + (Config == null ? 0 : 1); }
            get { return 0; }
        }

        public Node Match
        {
            get;
            private set;
        }

        public Id Binding
        {
            get;
            private set;
        }

        /*
        public Config Config
        {
            get;
            private set;
        }
        */

        public override NodeKind NodeKind
        {
            get { return NodeKind.ModelFact; }
        }

        internal ModelFact(ParserRuleContext context, Span span, Id binding, Node match)
            : base(context, span)
        {
            Contract.Requires(match != null && match.IsFuncOrAtom);
            Binding = binding;
            Match = match;
        }

        private ModelFact(ParserRuleContext context, ModelFact n, bool keepCompilerData)
            : base(context, n.Span)
        {
            CompilerData = keepCompilerData ? n.CompilerData : null;
        }

        
    }
}
