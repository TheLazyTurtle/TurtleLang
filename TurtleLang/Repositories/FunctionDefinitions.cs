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
        var result = FunctionNodesByName.TryGetValue(name, out var funcDef);

        return result && funcDef != null;
    }

    public static Dictionary<string, AstNode?> GetAll()
    {
        return FunctionNodesByName;
    }

    public static void Add(string name, AstNode? node)
    {
        if (!FunctionNodesByName.ContainsKey(name))
        {
            FunctionNodesByName.Add(name, node);
            return;
        }

        if (FunctionNodesByName[name] == null)
        {
            FunctionNodesByName[name] = node;
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
            Add(node.Key, node.Value);
        }
    }

    public static AstNode? Get(string name)
    {
        return FunctionNodesByName[name];
    }
}