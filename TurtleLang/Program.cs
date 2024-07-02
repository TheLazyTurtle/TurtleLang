using TurtleLang.Runtime;
using TurtleLang.Semantics;

namespace TurtleLang;

abstract class Program
{
    static void Main(string[] args)
    {
        var code = File.ReadAllText("Examples/Main.tl");
        InternalLogger.IsLoggingEnabled = false;
        InternalLogger.Log("================ Lexer ================");

        var lexer = new Lexer.Lexer();
        var tokens = lexer.Lex(code);
        foreach (var token in tokens)
            InternalLogger.Log(token);
        
        InternalLogger.Log("=============== Parser ===============");
        var parser = new Parser.Parser(tokens);
        var ast = parser.Parse();
        InternalLogger.Log(ast.ToString());

        var runner = new Runner();
        runner.LoadBuildInFunctions();
        
        InternalLogger.Log("=============== Semantic parsing ===============");
        var semanticParser = new SemanticParser(ast);
        semanticParser.Validate();
        
        InternalLogger.Log("=============== Runner ===============");
        runner.Run(ast);
    }
}