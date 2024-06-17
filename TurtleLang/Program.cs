using TurtleLang.Runtime;

namespace TurtleLang;

abstract class Program
{
    private static readonly Parser.Parser Parser = new();
    private static readonly Lexer.Lexer Lexer = new();
    private static readonly Runner Runner = new();
    
    static void Main(string[] args)
    {
        var code = File.ReadAllText("Examples/Main.tl");
        InternalLogger.IsLoggingEnabled = false;
        var tokens = Lexer.Lex(code);

        InternalLogger.Log("=============== Parser ===============");
        foreach (var token in tokens)
            InternalLogger.Log(token);
        
        InternalLogger.Log("================ Lexer ================");
        var ast = Parser.Parse(tokens);
        InternalLogger.Log(ast);
        
        InternalLogger.Log("=============== Runner ===============");
        Runner.Run(ast, Parser.FunctionNodesByName);
    }
}