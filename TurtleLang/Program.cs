using TurtleLang.Runtime;

namespace TurtleLang;

abstract class Program
{
    private static readonly Parser.Parser Parser = new();
    private static readonly Lexer.Lexer Lexer = new();
    private static readonly Runner Runner = new();
    
    static void Main(string[] args)
    {
        var tokens = Parser.Parse("Examples/Main.tl");

        Console.WriteLine("=============== Parser ===============");
        foreach (var token in tokens)
        {
            Console.WriteLine(token);
        }
        
        Console.WriteLine("================ Lexer ================");
        var ast = Lexer.Lex(tokens);
        Console.WriteLine(ast);
        
        Console.WriteLine("=============== Runner ===============");
        Runner.Run(ast, Lexer.FunctionNodesByName);
    }
}