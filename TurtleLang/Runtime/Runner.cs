using System.Diagnostics;
using TurtleLang.Models;
using TurtleLang.Models.Ast;
using TurtleLang.Models.Exceptions;
using StackFrame = TurtleLang.Models.StackFrame;

namespace TurtleLang.Runtime;

class Runner
{
    private readonly RuntimeStack _stack = new();
    private Dictionary<string, AstNode> _functionDefinitions;
    private AstNode _currentNode;
    private StackFrame? _stackFrameBeingBuild;
    
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
        if (!stackFrame.HasArguments())
            return;
        
        var argumentName = node.Value;
        stackFrame.CreateLocalVariable(argumentName, stackFrame.GetArgument());
        
        Console.WriteLine($"Method now has access to argument: {argumentName} with value: {stackFrame.GetLocalVariableByName(argumentName)}");
    }

    private void HandlePushArgument(AstNode node)
    {
        _stackFrameBeingBuild ??= new StackFrame();
        
        // It is an int it can never be an variable
        if (int.TryParse(node.Value, out var number))
        {
            _stackFrameBeingBuild.AddArgument(new RuntimeValue(PrimitiveTypes.Int, number));
            Console.WriteLine($"Adding int value of: {node.Value} to stackframe");
            return;
        }

        var currentFunctionStackFrame = _stack.Peek();
        
        // Handle string as input
        if (node.Value.StartsWith("\"") && node.Value.EndsWith("\"")) 
        {
            _stackFrameBeingBuild.AddArgument(new RuntimeValue(PrimitiveTypes.String, node.Value));
            
            Console.WriteLine($"Adding string value of: {node.Value} to stackframe");
            return;
        }

        // The current functions stackframe contains a local with this name so we can use this value.
        var localVariable = currentFunctionStackFrame.GetLocalVariableByName(node.Value);
        if (localVariable == null) 
            throw new VariableDoesNotExistException(node.Value);
        
        // We make a new one so we pass by value and not by reference. As these things are not objects they should not be passed as reference
        _stackFrameBeingBuild.AddArgument(new RuntimeValue(localVariable.Type, localVariable.Value));
        Console.WriteLine($"Passing value by value with value of: {node.Value} to stackframe");

    }

    private void HandleReturn(AstNode node)
    {
        var _ = _stack.Pop();
        Console.WriteLine("Popped stackframe");
        // Here we should also do something with the return value once we have implemented that
    }

    private void HandleCall(AstNode node)
    {
        if (IsBuildInFunction(node))
        {
            PushStackFrame();
            HandleBuildInFunction(node);
            return;
        }
        
        var functionNode = _functionDefinitions[node.Value] as FunctionDefinitionAstNode;
        Debug.Assert(functionNode != null);
        
        if (_stackFrameBeingBuild?.ArgumentCount > functionNode.ArgumentCount)
            throw new Exception("To many arguments given for function");
        
        if (_stackFrameBeingBuild?.ArgumentCount < functionNode.ArgumentCount)
            throw new Exception("To few arguments given for function");
        
        PushStackFrame();
        
        Console.WriteLine($"Executing function: {node.Value}");
        ExecuteNode(functionNode);
    }

    private void PushStackFrame()
    {
        if (_stackFrameBeingBuild != null)
        {
            _stack.Push(_stackFrameBeingBuild);
            _stackFrameBeingBuild = null;
            Console.WriteLine("Pushed stackframe");
            return;
        }

        // We always need a stackframe, so if there is non just push an empty one
        _stack.Push(new StackFrame());
        Console.WriteLine("Pushed empty stackframe");
    }

    private bool IsBuildInFunction(AstNode node)
    {
        return node.Value switch
        {
            "Print" => true,
            var _ => false
        };
    }

    private void HandleBuildInFunction(AstNode node)
    {
        switch (node.Value)
        {
            case "Print":
                HandlePrint();
                break;
        }
    }

    private void HandlePrint()
    {
        var stackFrame = _stack.Peek();
        if (!stackFrame.HasArguments())
            throw new Exception("Print must have at least one parameter");

        Console.WriteLine("Calling built in Print function");
        var count = 0;
        while (stackFrame.ArgumentCount > 0)
        {
            // As there can be an unlimited amount of variables we make them unique by an id
            var argumentName = $"{count++}";
            stackFrame.CreateLocalVariable(argumentName, stackFrame.GetArgument());
        }
        
        Console.WriteLine(string.Join(' ', stackFrame.GetAllLocals()));
    }
}