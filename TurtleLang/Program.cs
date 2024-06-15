using TurtleLang.Runtime;

namespace TurtleLang;

abstract class Program
{
    private static readonly Parser.Parser Parser = new();
    private static readonly Lexer.Lexer Lexer = new();
    private static readonly Runner Runner = new();
    
    static void Main(string[] args)
    {
        InternalLogger.IsLoggingEnabled = false;
        var tokens = Parser.Parse("Examples/Main.tl");

        InternalLogger.Log("=============== Parser ===============");
        foreach (var token in tokens)
            InternalLogger.Log(token);
        
        InternalLogger.Log("================ Lexer ================");
        var ast = Lexer.Lex(tokens);
        InternalLogger.Log(ast);
        
        InternalLogger.Log("=============== Runner ===============");
        Runner.Run(ast, Lexer.FunctionNodesByName);
    }
}