using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

using Antlr4.Runtime;

namespace Microsoft.Formula.AntlrParser.AST
{ 
    public sealed class Id : Node
    {
        private static readonly char[] splitChars = new char[] { '.' };

        public override int ChildCount
        {
            get { return 0; }
        }

        public string Name
        {
            get;
            private set;
        }

        public ImmutableArray<string> Fragments
        {
            get;
            private set;
        }

        public bool IsQualified
        {
            get { return Fragments.Length > 1; }
        }

        public override NodeKind NodeKind
        {
            get { return NodeKind.Id; }
        }

        internal Id(ParserRuleContext context, Span span, string name)
            : base(context, span)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Name = name;
            Fragments = ImmutableArray.ToImmutableArray(Name.Split(splitChars, StringSplitOptions.None));
        }

        private Id(ParserRuleContext context, Id node)
            : base(context, node.Span)
        {
            Name = node.Name;
            Fragments = ImmutableArray.ToImmutableArray(Name.Split(splitChars, StringSplitOptions.None));
        }
    }
}
