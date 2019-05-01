using System;
using Xunit;
using Xunit.Abstractions;
using Antlr4;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Microsoft.Formula.AntlrParser.AST;

namespace Microsoft.Formula.AntlrParser
{
    public class FormulaParserTests
    {
        private FormulaParser parser;
        private FormulaLexer lexer;
        private FormulaErrorListener errorListener;

        private readonly ITestOutputHelper output;

        public FormulaParserTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        private void Setup(string input)
        {
            ICharStream stream = new AntlrInputStream(input);
            lexer = new FormulaLexer(stream);
            errorListener = new FormulaErrorListener();
            ITokenStream tokens = new CommonTokenStream(lexer);
           
            parser = new FormulaParser(tokens);
            parser.BuildParseTree = true;
            ParserRuleContext context = parser.program();
            IParseTree tree = context.children[0];
            output.WriteLine(tree.ToStringTree());

            ExprVisitor visitor = new ExprVisitor();
            Node node = visitor.Visit(context);
        }

        [Fact]
        public void Test1()
        {
            string formulaText = @"
            domain Graph
            {
                V ::= new (id: Integer).
                E ::= new (src: V, dst: V).
  
                Reach ::= (V, V).
  
                Reach(x, y) :- E(x, y).
                Reach(x, z) :- E(x, y), Reach(y, z).
  
                conforms no Reach(x, x).
                conforms count({e | e is E}) >= 3.   
            }";

            Setup(formulaText);

            //Assert.Equal(parser.ToString(), "aaa");
            
        }
    }
}
