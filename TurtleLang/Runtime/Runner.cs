﻿using System.Diagnostics;
using TurtleLang.Models;
using TurtleLang.Models.Ast;
using TurtleLang.Repositories;
using StackFrame = TurtleLang.Models.StackFrame;

namespace TurtleLang.Runtime;

class Runner
{
    private readonly StackWithIndex<StackFrame> _stack = new();
    private StackFrame? _stackFrameBeingBuild;
    
    public void Run(AstTree ast)
    {
        var callMain = ast.Children.FirstOrDefault();
        Debug.Assert(callMain != null);
        Debug.Assert(callMain.GetValueAsString() == "Main");
        _stack.Push(new StackFrame
        {
            StackFrameType = StackFrameTypes.Global
        });
        ExecuteNode(callMain);
        
        Debug.Assert(_stack.Count == 1 && _stack.Peek().StackFrameType == StackFrameTypes.Global);
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
            case Opcode.If:
                HandleIfStatement(node);
                return;
            case Opcode.Else:
                Debug.Assert(false, "This should not happen as else should be handled by if");
                return;
            case Opcode.For:
                HandleFor(node);
                return;
        }

        if (node is not ScopeableAstNode scopeableAstNode)
            return;

        var children = scopeableAstNode.GetChildren();
        
        if (children == null)
            return;
        
        foreach (var child in children)
        {
            ExecuteNode(child);
        }
    }

    private void HandleFor(AstNode node)
    {
        if (node is not ForAstNode forNode)
            return;
        
        var stackFrame = new StackFrame
        {
            StackFrameType = StackFrameTypes.LocalScope
        };
        _stack.Push(stackFrame);

        if (forNode.Initializer == null)
            return;
        
        stackFrame.CreateLocalVariable(forNode.Initializer.Left.GetValueAsString()!, new RuntimeValue(BuildInTypes.Int, forNode.Initializer.Right!.GetValueAsInt()!));
        Console.WriteLine($"Creating local variable with name: {forNode.Initializer.Left.GetValueAsString()!} with value: {forNode.Initializer.Right!.GetValueAsInt()}");

        if (forNode.Expression == null)
            return;

        if (forNode.IncrementExpression == null)
            return;

        while (SolveExpression(forNode.Expression))
        {
            foreach (var child in forNode.Children)
            {
                ExecuteNode(child);
            }
            HandleIncrementOrDecrement(forNode.IncrementExpression);
        }

        _stack.Pop();
    }
    
    private void HandleIncrementOrDecrement(ExpressionAstNode node)
    {
        var leftRuntimeValue = GetRuntimeValueForExpressionHand(node.Left);
        if (leftRuntimeValue == null)
        {
            InterpreterErrorLogger.LogError($"Variable of {node.Left.GetValueAsString()} does not exist in scope");
            return;
        }
            
        Debug.Assert(node.ExpressionType is ExpressionTypes.Increase or ExpressionTypes.Decrease);
        Debug.Assert(leftRuntimeValue.Type == BuildInTypes.Int);

        var currentValue = leftRuntimeValue.GetValueAsInt();
        leftRuntimeValue.SetValueAsInt(currentValue + 1);
    }

    private void HandleIfStatement(AstNode node)
    {
        InternalLogger.Log("Handling if statement");
        if (node is not IfAstNode ifAstNode)
        {
            InterpreterErrorLogger.LogError("Node was not if node");
            return;
        }

        var expression = ifAstNode.Expression;
        var solved = SolveExpression(expression);

        // Handle else branch
        if (!solved)
        {
            if (ifAstNode.Else == null)
                return;
                    
            foreach (var child in ifAstNode.Else.Children)
            {
                ExecuteNode(child);
            }
            return;
        }

        // Handle if branch
        var children = ifAstNode.Children;
        foreach (var child in children)
        {
            ExecuteNode(child);
        }
    }

    private bool SolveExpression(ExpressionAstNode expressionAstNode)
    {
        var leftRuntimeValue = GetRuntimeValueForExpressionHand(expressionAstNode.Left);
        var rightRuntimeValue = GetRuntimeValueForExpressionHand(expressionAstNode.Right);

        if (leftRuntimeValue == null || rightRuntimeValue == null)
        {
            InterpreterErrorLogger.LogError("One of the variables in expression is null.");
            return false;
        }

        if (leftRuntimeValue.Type != rightRuntimeValue.Type)
        {
            InterpreterErrorLogger.LogError("Types do not match in expression");
            return false;
        }

        if (leftRuntimeValue.Type == BuildInTypes.Int)
            return CompareIntValues(leftRuntimeValue.GetValueAsInt(), rightRuntimeValue.GetValueAsInt(), expressionAstNode.ExpressionType);

        if (leftRuntimeValue.Type == BuildInTypes.Any)
        {
            Debug.Assert(false);
        }

        // Make string comparer
        return CompareStringValues(leftRuntimeValue.GetValueAsString(), rightRuntimeValue.GetValueAsString(), expressionAstNode.ExpressionType);
    }
    
    private bool CompareIntValues(int left, int right, ExpressionTypes expressionType)
    {
        return expressionType switch
        {
            ExpressionTypes.Eq => left == right,
            ExpressionTypes.Gt => left > right,
            ExpressionTypes.Gte => left >= right,
            ExpressionTypes.Lt => left < right,
            ExpressionTypes.Lte => left <= right,
            var _ => throw new ArgumentOutOfRangeException()
        };
    }

    private bool CompareStringValues(string left, string right, ExpressionTypes expressionType)
    {
        switch (expressionType)
        {
            case ExpressionTypes.Eq:
                return left == right;
            case ExpressionTypes.Gt:
            case ExpressionTypes.Gte:
            case ExpressionTypes.Lt:
            case ExpressionTypes.Lte:
                InterpreterErrorLogger.LogError($"String does int implement {expressionType}");
                return false;
            case var _:
                throw new ArgumentOutOfRangeException();
        }
    }

    private RuntimeValue? GetRuntimeValueForExpressionHand(AstNode node)
    {
        if (node is VariableAstNode variableNode)
        {
            var count = 0;
            var stackFrame = _stack.PeekAtIndex(count);
            while (stackFrame.GetLocalVariableByName(variableNode.GetValueAsString()!) == null)
            {
                stackFrame = _stack.PeekAtIndex(++count);
                
                if (stackFrame.StackFrameType == StackFrameTypes.Function)
                    break;
            }
            return stackFrame.GetLocalVariableByName(variableNode.GetValueAsString()!);
        }

        if (node is ValueAstNode valueNode)
        {
            switch (valueNode.Type)
            {
                case BuildInTypes.Int:
                    return new RuntimeValue(valueNode.Type, valueNode.GetValueAsInt()!);
                case BuildInTypes.String:
                    return new RuntimeValue(valueNode.Type, valueNode.GetValueAsString()!);
                case BuildInTypes.Any:
                default:
                    Debug.Assert(false);
                    throw new ArgumentOutOfRangeException();
            }
        }

        return null;
    }
    
    private void HandleLoadArgument(FunctionDefinitionAstNode node)
    {
        var stackFrame = _stack.Peek();
        if (!stackFrame.HasArguments())
            return;
        
        if (node.Arguments == null)
            return;

        foreach (var argument in node.Arguments)
        {
            var argumentName = argument.Name;
            stackFrame.CreateLocalVariable(argumentName, stackFrame.GetArgument());
            InternalLogger.Log($"Method now has access to argument: {argumentName} with value: {stackFrame.GetLocalVariableByName(argumentName)}");
        }
    }

    private void HandleAddArgument(AstNode node)
    {
        _stackFrameBeingBuild ??= new StackFrame
        {
            StackFrameType = StackFrameTypes.Function
        };
        
        // It is an int it can never be an variable
        var intValue = node.GetValueAsInt();
        if (intValue != null)
        {
            _stackFrameBeingBuild.AddArgument(new RuntimeValue(BuildInTypes.Int, intValue));
            InternalLogger.Log($"Adding int value of: {node.GetValueAsInt()} to stackframe");
            return;
        }
        
        // Handle variable as input
        if (node is VariableAstNode variableNode)
        {
            var currentFunctionStackFrame = _stack.Peek();

            // The current functions stackframe contains a local with this name so we can use this value.
            var localVariable = currentFunctionStackFrame.GetLocalVariableByName(variableNode.GetValueAsString()!);
            if (localVariable == null)
                InterpreterErrorLogger.LogError($"Variable {variableNode.GetValueAsString()} does not exist", variableNode);
        
            // We make a new one so we pass by value and not by reference. As these things are not objects they should not be passed as reference
            _stackFrameBeingBuild.AddArgument(new RuntimeValue(localVariable.Type, localVariable.Value));
            InternalLogger.Log($"Passing value by value with value of: {localVariable} to stackframe");
            
            return;
        }
        

        // Handle string as input
        if (node is not ValueAstNode argNode) 
            return;
        _stackFrameBeingBuild.AddArgument(new RuntimeValue(BuildInTypes.String, argNode.GetValueAsString()!));
            
        InternalLogger.Log($"Adding string value of: {argNode.GetValueAsString()} to stackframe");
    }

    private void HandleReturn(AstNode node)
    {
        var _ = _stack.Pop();
        InternalLogger.Log("Popped stackframe");
        // Here we should also do something with the return value once we have implemented that
    }

    private void HandleCall(AstNode node)
    {
        // TODO: Convert this into something that handles all the argument loading etc in here and not through an extra fn call 
        // TODO: We can probably do add argument and load argument into one thing that will do everything and prevent doing everything twice
        foreach (var child in node.Children)
        {
            Debug.Assert(child.Opcode is Opcode.Value or Opcode.Variable);
            HandleAddArgument(child);
        }
        _stack.Push(_stackFrameBeingBuild ?? new StackFrame{StackFrameType = StackFrameTypes.Function});
        InternalLogger.Log("Pushed stackframe");
        _stackFrameBeingBuild = null;
        
        var stackFrame = _stack.Peek();

        var functionDefinition = FunctionDefinitions.Get(node.GetValueAsString());

        if (functionDefinition is FunctionDefinitionAstNode functionNode)
        {
            Debug.Assert(functionNode != null);
            
            if (stackFrame.ArgumentCount > functionNode.ArgumentCount)
                InterpreterErrorLogger.LogError("To many arguments given for function", node);
            
            if (stackFrame.ArgumentCount < functionNode.ArgumentCount)
                InterpreterErrorLogger.LogError("To few arguments given for function", node);
            
            InternalLogger.Log($"Executing function: {node.GetValueAsString()}");
            HandleLoadArgument(functionNode);
            ExecuteNode(functionNode);
            return;
        }

        if (functionDefinition is not BuildInFunctionAstNode buildInFunctionNode) 
            return;
        buildInFunctionNode.Handler(node);
    }

    public void LoadBuildInFunctions()
    {
        FunctionDefinitions.AddRange( new Dictionary<string, AstNode>
        {
            {"Print", new BuildInFunctionAstNode("Print", BuildInPrint, true)}
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