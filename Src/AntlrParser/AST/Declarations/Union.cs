using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Antlr4.Runtime;

namespace Microsoft.Formula.AntlrParser.AST
{
    public sealed class Union : Node
    {
        private ImmutableArray<Node> components;

        public override int ChildCount
        {
            get { return components.Length; }
        }

        public ImmutableArray<Node> Components
        {
            get;
            private set;
        }

        internal Union(ParserRuleContext context, Span span)
            : base(context, span)
        {
            components = new ImmutableArray<Node>();
        }

        private Union(ParserRuleContext context, Union n, bool keepCompilerData)
            : base(context, n.Span)
        {
            CompilerData = keepCompilerData ? n.CompilerData : null;
        }

        

        public override NodeKind NodeKind
        {
            get { return NodeKind.Union; }
        }

        internal void AddComponent(Node n, bool addLast = true)
        {

        }
    }
}