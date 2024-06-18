
using System.Diagnostics;
using TurtleLang.Models;
using TurtleLang.Models.Ast;

namespace TurtleLang;

class InterpreterErrorLogger
{
    public static void LogError(string error)
    {
        Console.WriteLine(error);
        
        Debug.Assert(false);
        Environment.Exit(-1);
        throw new Exception(); // Fake exception to trick lsp etc
    }
    
    public static void LogError(string error, Token? token)
    {
        if (token == null)
        {
            LogError(error);
            return;
        }

        LogError($"{error} on line: {token.LineNumber}");
    }
    
    public static void LogError(string error, AstNode node)
    {
        LogError($"{error} on line: {node.LineNumber}");
    }
}