using System.Diagnostics;
using TurtleLang.Models;
using TurtleLang.Models.Ast;
using StackFrame = TurtleLang.Models.StackFrame;

namespace TurtleLang.Runtime;

class Runner
{
    private readonly RuntimeStack _stack = new();
    private Dictionary<string, AstNode> _functionDefinitions;
    private AstNode _currentNode;
    private StackFrame? _currentStackFrame;
    
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
            case Opcode.LoadArgument:
                HandleLoadArgument(node);
                break;
            case Opcode.PushArgument:
                HandlePushArgument(node);
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

    private void HandleLoadArgument(AstNode node)
    {
        var stackFrame = _stack.Peek();
        var argumentName = node.Value;
        stackFrame.CreateLocalVariable(argumentName, stackFrame.GetArgument());
        // if (item.Opcode != node.Opcode || item.Value != node.Value)
        //     throw new Exception("The stack did not stay straight");

        // Console.WriteLine($"Method now has access to value: {item.Value}");
    }

    private void HandlePushArgument(AstNode node)
    {
        _currentStackFrame ??= new StackFrame();
        _currentStackFrame.AddArgument(new RuntimeValue(PrimitiveTypes.Int, node.Value));
        
        Console.WriteLine($"Adding value of: {node.Value} to stackframe");
    }

    private void HandleReturn(AstNode node)
    {
        var item = _stack.Pop();
        Console.WriteLine("Popped stackframe");
        // Here we should also do something with the return value once we have implemented that
    }

    private void HandleCall(AstNode node)
    {
        if (_currentStackFrame != null)
        {
            _stack.Push(_currentStackFrame);
            _currentStackFrame = null;
            Console.WriteLine("Pushed stackframe");
        }
        else
        {
            // We always need a stackframe, so if there is non just push an empty one
            _stack.Push(new StackFrame());
            Console.WriteLine("Pushed empty stackframe");
        }
        Console.WriteLine($"Executing function: {node.Value}");
        var functionNode = _functionDefinitions[node.Value];
        ExecuteNode(functionNode);
    }
}