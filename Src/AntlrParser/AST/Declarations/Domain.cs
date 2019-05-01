using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

using Antlr4.Runtime;

namespace Microsoft.Formula.AntlrParser.AST
{
    public sealed class Domain : Node
    {
        //private LinkedList<ModRef> compositions;
        //private LinkedList<Rule> rules;
        private LinkedList<Node> typeDecls;
        //private LinkedList<ContractItem> conforms;

        
        public override int ChildCount
        {
            //get { return compositions.Count + rules.Count + typeDecls.Count + conforms.Count + 1; }
            get { return 0; }
        }

        public override NodeKind NodeKind
        {
            get { return NodeKind.Domain; }
        }

        internal Domain(ParserRuleContext context, Span span) 
            : base(context, span)
        {

        }

        /*
        public string Name
        {
            get;
            private set;
        }

        public ImmutableCollection<Rule> Rules
        {
            get;
            private set;
        }

        public ImmutableCollection<Node> TypeDecls
        {
            get;
            private set;
        }

        public ImmutableCollection<ContractItem> Conforms
        {
            get;
            private set;
        }

        public Config Config
        {
            get;
            private set;
        }

        

        public ImmutableCollection<ModRef> Compositions
        {
            get;
            private set;
        }

        public ComposeKind ComposeKind
        {
            get;
            private set;
        }
        */


    }
}
