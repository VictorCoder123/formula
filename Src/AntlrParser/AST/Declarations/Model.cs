using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

using Antlr4.Runtime;

namespace Microsoft.Formula.AntlrParser.AST
{

    public sealed class Model : Node
    {
        //private LinkedList<ModRef> includes;

        //private LinkedList<ContractItem> contracts;

        private LinkedList<ModelFact> facts;

        public override int ChildCount
        {
            //get { return 1 + includes.Count + contracts.Count + facts.Count; }
            get { return 0; }
        }

        public override NodeKind NodeKind
        {
            get { return NodeKind.Model; }
        }

        public string Name
        {
            get;
            private set;
        }

        public bool IsPartial
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

        public ModRef Domain
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

        public ImmutableCollection<ContractItem> Contracts
        {
            get;
            private set;
        }

        public ImmutableCollection<ModelFact> Facts
        {
            get;
            private set;
        }
        */


        private Model(ParserRuleContext context, Model n, bool keepCompilerData)
            : base(context, n.Span)
        {
            Name = n.Name;
            //ComposeKind = n.ComposeKind;
            IsPartial = n.IsPartial;
            CompilerData = keepCompilerData ? n.CompilerData : null;
        }     

        internal void AddFact(ModelFact f, bool addLast = true)
        {
            Contract.Requires(f != null);
            if (addLast)
            {
                facts.AddLast(f);
            }
            else
            {
                facts.AddFirst(f);
            }
        }
    }
}
