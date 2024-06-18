using TurtleLang.Models.Ast;
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
        var tokens = Lexer.Lex(code);

        // InternalLogger.Log("================ Lexer ================");
        // foreach (var token in tokens)
        //     InternalLogger.Log(token);
        
        InternalLogger.Log("=============== Parser ===============");
        _parser = new Parser.Parser(tokens);
        var ast = _parser.Parse();

        InternalLogger.Log(ast.ToString());
        // foreach (var node in _parser.FunctionNodesByName)
        // {
        //     var funcDef = (FunctionDefinitionAstNode)node.Value!;
        //     InternalLogger.Log(funcDef);
        // }

        // InternalLogger.Log("=============== Runner ===============");
        // Runner.Run(ast, Parser.FunctionNodesByName);
    }
}