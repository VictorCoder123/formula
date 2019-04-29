using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace AntlrParser
{
    class Program
    {
        public static void Main(string[] args)
        {
            String input = "";
            ICharStream stream = new AntlrInputStream(input);
            ITokenSource lexer = new FormulaLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            FormulaParser parser = new FormulaParser(tokens);
            parser.BuildParseTree = true;
            
            
        }
    }
}
