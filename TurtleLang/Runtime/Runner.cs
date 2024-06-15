using System.Diagnostics;
using TurtleLang.Models;
using TurtleLang.Models.Ast;

namespace TurtleLang.Runtime;

class Runner
{
    private readonly RuntimeStack _stack = new();
    private Dictionary<string, AstNode> _functionDefinitions;
    private AstNode _currentNode;
    
    public void Run(AstTree ast, Dictionary<string, AstNode> functionDefinitions)
    {
        _functionDefinitions = functionDefinitions;
        var root = ast.Root;
        Debug.Assert(root is { Opcode: Opcode.Call });
        _currentNode = root;
        
        ExecuteNode(_currentNode);
    }

    private void ExecuteNode(AstNode node)
    {
        if (node.Opcode is Opcode.Eof)
            return;
        
        switch (node.Opcode)
        {
            case Opcode.Call:
                HandleCall(node);
                break;
            case Opcode.Return:
                HandleReturn(node);
                break;
        }
        
        if (node.Child != null)
        {
            ExecuteNode(node.Child);
        }

        if (node.Sibling != null)
        {
            if (node.Sibling.Opcode != Opcode.FunctionDefinition)
                ExecuteNode(node.Sibling);
        }
    }

    private void HandleReturn(AstNode node)
    {
        _stack.Pop();
    }

    private void HandleCall(AstNode node)
    {
        var functionNode = _functionDefinitions[node.Value];
        Console.WriteLine($"Executing function: {node.Value}");
        _stack.Push(node);
        ExecuteNode(functionNode);
    }
}