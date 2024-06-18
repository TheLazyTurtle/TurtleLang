using System.Diagnostics;
using TurtleLang.Models;
using TurtleLang.Models.Ast;
using StackFrame = TurtleLang.Models.StackFrame;

namespace TurtleLang.Runtime;

class Runner
{
    private readonly Stack<StackFrame> _stack = new();
    private Dictionary<string, AstNode?> _functionDefinitions;
    
    public void Run(AstTree ast, Dictionary<string, AstNode?> functionDefinitions)
    {
        _functionDefinitions = functionDefinitions;

        var callMain = ast.Children.FirstOrDefault();
        Debug.Assert(callMain != null);
        Debug.Assert(callMain.Value == "Main");
        ExecuteNode(callMain);
        
        Debug.Assert(_stack.Count == 0);
    }

    private void ExecuteNode(AstNode node)
    {
        if (node.Opcode is Opcode.Eof)
            return;
        
        switch (node.Opcode)
        {
            case Opcode.Call:
                HandleCall(node);
                return; // After a call it should not do anything
            case Opcode.Return:
                HandleReturn(node);
                break;
            case Opcode.LoadArgument:
                HandleLoadArgument(node);
                break;
            case Opcode.PushArgument:
                throw new Exception("Pushing arguments is only allowed in a call");
        }

        foreach (var child in node.Children)
        {
            ExecuteNode(child);
        }
        
        // if (node.Children != null)
        //     ExecuteNode(node.Children);
        //
        // if (node.Sibling == null) 
        //     return;
        //
        // if (node.Sibling.Opcode != Opcode.FunctionDefinition)
        //     ExecuteNode(node.Sibling);
    }

    private void HandleLoadArgument(AstNode node)
    {
        var stackFrame = _stack.Peek();
        if (!stackFrame.HasArguments())
            return;
        
        var argumentName = node.Value;
        stackFrame.CreateLocalVariable(argumentName, stackFrame.GetArgument());
        
        InternalLogger.Log($"Method now has access to argument: {argumentName} with value: {stackFrame.GetLocalVariableByName(argumentName)}");
    }

    private void HandlePushArgument(StackFrame stackFrame, AstNode node)
    {
        // It is an int it can never be an variable
        if (int.TryParse(node.Value, out var number))
        {
            stackFrame.AddArgument(new RuntimeValue(PrimitiveTypes.Int, number));
            InternalLogger.Log($"Adding int value of: {node.Value} to stackframe");
            return;
        }

        // Handle string as input
        if (node is ArgumentAstNode argNode)
        {
            stackFrame.AddArgument(new RuntimeValue(PrimitiveTypes.String, argNode.Value));
            
            InternalLogger.Log($"Adding string value of: {argNode.Value} to stackframe");
            return;
        }
        
        // Handle passing a variable
        var currentFunctionStackFrame = _stack.Peek();

        // The current functions stackframe contains a local with this name so we can use this value.
        var localVariable = currentFunctionStackFrame.GetLocalVariableByName(node.Value);
        if (localVariable == null) 
            InterpreterErrorLogger.LogError($"Variable {node.Value} does not exist", node);
        
        // We make a new one so we pass by value and not by reference. As these things are not objects they should not be passed as reference
        stackFrame.AddArgument(new RuntimeValue(localVariable.Type, localVariable.Value));
        InternalLogger.Log($"Passing value by value with value of: {node.Value} to stackframe");
    }

    private void HandleReturn(AstNode node)
    {
        var _ = _stack.Pop();
        InternalLogger.Log("Popped stackframe");
        // Here we should also do something with the return value once we have implemented that
    }

    private void LoadAllArguments(AstNode node, StackFrame stackFrame)
    {
        foreach (var child in node.Children)
        {
            if (child.Opcode != Opcode.PushArgument)
            {
                InternalLogger.Log($"Did not expect children of call to be anything else than PushArgument. Got {child.Opcode}");
                continue;
            }
            HandlePushArgument(stackFrame, child);
        }
    }

    private void HandleCall(AstNode node)
    {
        var stackFrame = new StackFrame();
        
        LoadAllArguments(node, stackFrame);
        if (IsBuildInFunction(node))
        {
            PushStackFrame(stackFrame);
            HandleBuildInFunction(node);
            return;
        }

        var functionNode = _functionDefinitions[node.Value] as FunctionDefinitionAstNode;
        Debug.Assert(functionNode != null);
        
        if (stackFrame.ArgumentCount > functionNode.ArgumentCount)
            InterpreterErrorLogger.LogError("To many arguments given for function", node);
        
        if (stackFrame.ArgumentCount < functionNode.ArgumentCount)
            InterpreterErrorLogger.LogError("To few arguments given for function", node);
        
        PushStackFrame(stackFrame);
        
        InternalLogger.Log($"Executing function: {node.Value}");
        ExecuteNode(functionNode);
    }

    private void PushStackFrame(StackFrame stackFrame)
    {
        if (stackFrame.ArgumentCount != 0)
        {
            _stack.Push(stackFrame);
            InternalLogger.Log("Pushed stackframe");
            return;
        }

        // We always need a stackframe, so if there is non just push an empty one
        _stack.Push(new StackFrame());
        InternalLogger.Log("Pushed empty stackframe");
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
                HandlePrint(node);
                break;
        }
    }

    private void HandlePrint(AstNode node)
    {
        InternalLogger.Log("Calling built in Print function");
        
        var stackFrame = _stack.Pop();
        
        if (!stackFrame.HasArguments())
            InterpreterErrorLogger.LogError("Print must have at least one parameter", node);

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