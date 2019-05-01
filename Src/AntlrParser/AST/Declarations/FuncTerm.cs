using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

using Antlr4.Runtime;

namespace Microsoft.Formula.AntlrParser.AST
{
    public sealed class FuncTerm : Node
    {
        private LinkedList<Node> args;

        public override int ChildCount
        {
            get { return args.Count + (Function is Node ? 1 : 0); }
        }

        public object Function
        {
            get;
            private set;
        }

        public override NodeKind NodeKind
        {
            get { return NodeKind.FuncTerm; }
        }

        public ImmutableArray<Node> Args
        {
            get;
            private set;
        }

        /*
        internal FuncTerm(ParserRuleContext context, Span span, Id cons)
            : base(context, span)
        {
            Contract.Requires(cons != null);
            OpKind kind;

            if (ASTQueries.ASTSchema.Instance.TryGetOpKind(cons.Name, out kind))
            {
                Function = kind;
            }
            else
            {
                Function = cons;
            }

            args = new LinkedList<Node>();
            Args = ImmutableArray.ToImmutableArray(args);
        }
        */

        internal FuncTerm(ParserRuleContext context, Span span, OpKind op)
            : base(context, span)
        {
            Function = op;
            args = new LinkedList<Node>();
            Args = ImmutableArray.ToImmutableArray(args);
        }

        private FuncTerm(ParserRuleContext context, FuncTerm n, bool keepCompilerData)
            : base(context, n.Span)
        {
            if (n.Function is OpKind)
            {
                Function = (OpKind)n.Function;
            }

            CompilerData = keepCompilerData ? n.CompilerData : null;
        }

        internal void AddArg(Node n, bool addLast = true)
        {
            Contract.Requires(n != null && n.IsFuncOrAtom);

            if (addLast)
            {
                args.AddLast(n);
            }
            else
            {
                args.AddFirst(n);
            }
        }        
    }
}
