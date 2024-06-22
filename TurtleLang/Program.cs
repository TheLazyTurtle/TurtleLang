using TurtleLang.Runtime;

namespace TurtleLang;

abstract class Program
{
    private static Parser.Parser _parser;
    private static readonly Lexer.Lexer Lexer = new();
    private static readonly Runner Runner = new();
    
    static void Main(string[] args)
    {
        var code = File.ReadAllText("Examples/Main.tl");
        InternalLogger.IsLoggingEnabled = true;
        InternalLogger.Log("================ Lexer ================");
        
        var tokens = Lexer.Lex(code);
        foreach (var token in tokens)
            InternalLogger.Log(token);
        
        InternalLogger.Log("=============== Parser ===============");
        _parser = new Parser.Parser(tokens);
        var ast = _parser.Parse();
        InternalLogger.Log(ast.ToString());
        
        // TODO: Place a function here that validates that all called functions actually exist
        
        InternalLogger.Log("=============== Runner ===============");
        Runner.LoadBuildInFunctions();
        Runner.Run(ast);
    }
}