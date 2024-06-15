
using TurtleLang.Models;
using TurtleLang.Models.Ast;

namespace TurtleLang;

class InterpreterErrorLogger
{
    public static void LogError(string error)
    {
        Console.WriteLine(error);
        Environment.Exit(-1);
        throw new Exception();
    }
    
    public static void LogError(string error, Token token)
    {
        LogError($"{error} on line: {token.LineNumber}");
    }
    
    public static void LogError(string error, AstNode node)
    {
        LogError($"{error} on line: {node.LineNumber}");
    }
}