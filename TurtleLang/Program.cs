using TurtleLang.Runtime;
using TurtleLang.Semantics;

namespace TurtleLang;

abstract class Program
{
    static void Main(string[] args)
    {
        var code = File.ReadAllText("Examples/Main.tl");
        InternalLogger.IsLoggingEnabled = true;
        InternalLogger.Log("================ Lexer ================");

        var lexer = new Lexer.Lexer();
        var tokens = lexer.Lex(code);
        foreach (var token in tokens)
            InternalLogger.Log(token);
        
        InternalLogger.Log("=============== Parser ===============");
        var parser = new Parser.Parser(tokens);
        var ast = parser.Parse();
        InternalLogger.Log(ast.ToString());

        var semanticParser = new SemanticParser(ast);
        semanticParser.Validate();

    //     InternalLogger.Log("=============== Runner ===============");
    //     var runner = new Runner();
    //     runner.LoadBuildInFunctions();
    //     runner.Run(ast);
    }
}