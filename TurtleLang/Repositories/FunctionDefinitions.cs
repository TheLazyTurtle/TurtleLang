using TurtleLang.Models.Ast;

namespace TurtleLang.Repositories;

static class FunctionDefinitions
{
    private static readonly Dictionary<string, AstNode?> FunctionNodesByName;
    public static int Count => FunctionNodesByName.Count;

    static FunctionDefinitions()
    {
        FunctionNodesByName = new Dictionary<string, AstNode?>();
    }

    public static bool Contains(string name)
    {
        return FunctionNodesByName.ContainsKey(name);
    }

    public static void Add(string name, AstNode node)
    {
        if (!FunctionNodesByName.ContainsKey(name))
        {
            FunctionNodesByName.Add(name, node);
            return;
        }

        if (FunctionNodesByName[name] == null)
        {
            FunctionNodesByName.Add(name, node);
        }
        else
        {
            InterpreterErrorLogger.LogError($"Redefinition of function with name: {node.GetValueAsString()}");
        }
    }

    public static void AddRange(Dictionary<string, AstNode> nodes)
    {
        foreach (var node in nodes)
        {
            FunctionNodesByName.Add(node.Key, node.Value);
        }
    }

    public static AstNode? Get(string name)
    {
        return FunctionNodesByName[name];
    }
}