using System.Diagnostics;
using TurtleLang.Models;
using TurtleLang.Models.Ast;
using TurtleLang.Repositories;
using StackFrame = TurtleLang.Models.StackFrame;

namespace TurtleLang.Runtime;

class Runner
{
    private readonly Stack<StackFrame> _stack = new();
    private StackFrame? _stackFrameBeingBuild = null;
    
    public void Run(AstTree ast)
    {
        var callMain = ast.Children.FirstOrDefault();
        Debug.Assert(callMain != null);
        Debug.Assert(callMain.Value == "Main");
        _stack.Push(new StackFrame());
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
                return;
            case Opcode.LoadArgument:
                HandleLoadArgument(node);
                return;
            case Opcode.AddArgument:
                HandleAddArgument(node);
                return;
            case Opcode.PushStackFrame:
                _stack.Push(_stackFrameBeingBuild!);
                InternalLogger.Log("Pushed stackframe");
                _stackFrameBeingBuild = null;
                return;
        }

        foreach (var child in node.Children)
        {
            ExecuteNode(child);
        }
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

    private void HandleAddArgument(AstNode node)
    {
        _stackFrameBeingBuild ??= new StackFrame();
        
        // It is an int it can never be an variable
        if (int.TryParse(node.Value, out var number))
        {
            _stackFrameBeingBuild.AddArgument(new RuntimeValue(PrimitiveTypes.Int, number));
            InternalLogger.Log($"Adding int value of: {node.Value} to stackframe");
            return;
        }

        // Handle string as input
        if (node is ArgumentAstNode argNode)
        {
            _stackFrameBeingBuild.AddArgument(new RuntimeValue(PrimitiveTypes.String, argNode.Value));
            
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
        _stackFrameBeingBuild.AddArgument(new RuntimeValue(localVariable.Type, localVariable.Value));
        InternalLogger.Log($"Passing value by value with value of: {localVariable} to stackframe");
    }

    private void HandleReturn(AstNode node)
    {
        var _ = _stack.Pop();
        InternalLogger.Log("Popped stackframe");
        // Here we should also do something with the return value once we have implemented that
    }

    private void HandleCall(AstNode node)
    {
        var stackFrame = _stack.Peek();

        var functionDefinition = FunctionDefinitions.Get(node.Value);

        if (functionDefinition is FunctionDefinitionAstNode functionNode)
        {
            Debug.Assert(functionNode != null);
            
            if (stackFrame.ArgumentCount > functionNode.ArgumentCount)
                InterpreterErrorLogger.LogError("To many arguments given for function", node);
            
            if (stackFrame.ArgumentCount < functionNode.ArgumentCount)
                InterpreterErrorLogger.LogError("To few arguments given for function", node);
            
            InternalLogger.Log($"Executing function: {node.Value}");
            ExecuteNode(functionNode);
            return;
        }

        if (functionDefinition is not BuildInFunctionAstNode buildInFunctionNode) 
            return;
        buildInFunctionNode.Handler(node);
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

    public void LoadBuildInFunctions()
    {
        FunctionDefinitions.AddRange( new Dictionary<string, AstNode>
        {
            {"Print", new BuildInFunctionAstNode("Print", BuildInPrint, new List<string>{"textToPrint"})}
        });
    }

    private void BuildInPrint(AstNode node)
    {
        InternalLogger.Log("Calling built in Print function");
        
        var stackFrame = _stack.Pop();
        InternalLogger.Log("Popped stack in print");

        var count = 0;
        while (stackFrame.HasArguments())
        {
            stackFrame.CreateLocalVariable($"{count}", stackFrame.GetArgument());
            count++;
        }
        
        Console.WriteLine(string.Join(',', stackFrame.GetAllLocals()));
    }
}