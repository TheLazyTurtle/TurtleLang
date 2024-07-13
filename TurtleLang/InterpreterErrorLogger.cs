
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using TurtleLang.Models;
using TurtleLang.Models.Ast;

namespace TurtleLang;

class InterpreterErrorLogger
{
    [DoesNotReturn]
    public static void LogError(string error)
    {
        Console.WriteLine(error);
        
        Debug.Assert(false);
        Environment.Exit(-1);
    }
    
    [DoesNotReturn]
    public static void LogError(string error, Token? token)
    {
        if (token == null)
        {
            LogError(error);
            return;
        }

        LogError($"{error} on line: {token.LineNumber}");
    }
    
    [DoesNotReturn]
    public static void LogError(string error, AstNode node)
    {
        LogError($"{error} on line: {node.LineNumber}");
    }
}