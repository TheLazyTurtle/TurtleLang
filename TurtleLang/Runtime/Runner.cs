using System.Diagnostics;
using TurtleLang.Models;
using TurtleLang.Models.Ast;
using TurtleLang.Models.Types;
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
            case Opcode.Expression:
                HandleExpression(node);
                return;
        }

        if (node is not ScopeableAstNode scopeableAstNode)
            return;
        
        LoadLocals(scopeableAstNode);

        var children = scopeableAstNode.GetChildren();
        
        foreach (var child in children)
        {
            ExecuteNode(child);
        }
    }

    private void HandleExpression(AstNode node)
    {
        if (node is not ExpressionAstNode expressionNode)
            return;
        
        Debug.Assert(expressionNode.ExpressionType == ExpressionTypes.Assign);
        var stackFrame = _stack.Peek();
        var variable = stackFrame.GetLocalVariableByName(expressionNode.Left.GetValueAsString()!);

        if (variable == null)
        {
            InterpreterErrorLogger.LogError($"Variable: {expressionNode.Left} does not exist in current scope");
            return;
        }

        var valueToSet = expressionNode.Right!.Proxy_GetRawValue();
        variable.Proxy_SetRawValue(valueToSet);
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
        
        stackFrame.CreateLocalVariable(forNode.Initializer.Left.GetValueAsString()!, new RuntimeValue(new IntTypeDefinition(), forNode.Initializer.Right!.GetValueAsInt()!));
        InternalLogger.Log($"Creating local variable with name: {forNode.Initializer.Left.GetValueAsString()!} with value: {forNode.Initializer.Right!.GetValueAsInt()}");

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
        Debug.Assert(leftRuntimeValue.Type is IntTypeDefinition);

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

        if (!leftRuntimeValue.Type.Equals(rightRuntimeValue.Type))
        {
            InterpreterErrorLogger.LogError("Types do not match in expression");
            return false;
        }

        if (leftRuntimeValue.Type is IntTypeDefinition)
            return CompareIntValues(leftRuntimeValue.GetValueAsInt(), rightRuntimeValue.GetValueAsInt(), expressionAstNode.ExpressionType);

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

        if (node is not ValueAstNode valueNode) 
            return null;
        
        if (valueNode.Type is IntTypeDefinition)
            return new RuntimeValue(valueNode.Type, valueNode.GetValueAsInt()!);
            
        if (valueNode.Type is StringTypeDefinition)
            return new RuntimeValue(valueNode.Type, valueNode.GetValueAsString()!);
            
        throw new ArgumentOutOfRangeException();
    }
    
    private void HandleLoadArgument(FunctionDefinitionAstNode node)
    {
        var stackFrame = _stack.Peek();
        
        if (node.Arguments == null)
            return;

        foreach (var argument in node.Arguments)
        {
            var argumentName = argument.GetValueAsString();
            stackFrame.CreateLocalVariable(argumentName, stackFrame.ConsumeArgument());
            InternalLogger.Log($"Method now has access to argument: {argumentName} with value: {stackFrame.GetLocalVariableByName(argumentName)}");
        }
    }

    private void HandleAddArguments(AstNode node)
    {
        _stackFrameBeingBuild ??= new StackFrame
        {
            StackFrameType = StackFrameTypes.Function
        };
        
        foreach (var child in node.Children)
        {
            Debug.Assert(child.Opcode is Opcode.Value or Opcode.Variable);

            if (child is VariableAstNode variableNode)
            {
                var currentFunctionStackFrame = _stack.Peek();

                // The current functions stackframe contains a local with this name so we can use this value.
                var localVariable = currentFunctionStackFrame.GetLocalVariableByName(variableNode.GetValueAsString()!);
                if (localVariable == null)
                    InterpreterErrorLogger.LogError($"Variable {variableNode.GetValueAsString()} does not exist", variableNode);

                if (localVariable.Type is StructDefinition)
                {
                    _stackFrameBeingBuild.AddArgument(localVariable);
                    InternalLogger.Log($"Passing value by reference with type of: {localVariable} to stackframe");
                }
                else
                {
                    // We make a new one so we pass by value and not by reference. As these things are not objects they should not be passed as reference
                    _stackFrameBeingBuild.AddArgument(new RuntimeValue(localVariable.Type, localVariable.Value));
                    InternalLogger.Log($"Passing value by value with value of: {localVariable} to stackframe");
                }

                continue;
            }

            if (child is not ValueAstNode valueNode) 
                throw new Exception("Unexpected node");
            
            _stackFrameBeingBuild.AddArgument(new RuntimeValue(valueNode.Type, valueNode.Proxy_GetRawValue()));
            InternalLogger.Log($"Added argument: {valueNode.Type}");
        }
    }

    private void HandleReturn(AstNode node)
    {
        var _ = _stack.Pop();
        InternalLogger.Log("Popped stackframe");
        // Here we should also do something with the return value once we have implemented that
    }
    
    private void LoadLocals(ScopeableAstNode astNode)
    {
        var stackFrame = _stack.Peek();
        if (astNode is not FunctionDefinitionAstNode funcDef)
            throw new Exception("Why is this not funcDef");
        
        foreach (var local in funcDef.Locals)
        {
            stackFrame.CreateLocalVariable(local.GetValueAsString(), new RuntimeValue(local.Type, null));
        }
    }

    private void HandleCall(AstNode node)
    {
        // This pushes from call
        HandleAddArguments(node);
        
        var stackFrame = _stackFrameBeingBuild;
        _stack.Push(_stackFrameBeingBuild ?? new StackFrame{StackFrameType = StackFrameTypes.Function});
        InternalLogger.Log("Pushed stackframe");
        _stackFrameBeingBuild = null;

        var functionDefinition = FunctionDefinitions.Get(node.GetValueAsString());

        if (functionDefinition is FunctionDefinitionAstNode functionNode)
        {
            Debug.Assert(functionNode != null);
            
            InternalLogger.Log($"Executing function: {node.GetValueAsString()}");
            // This loads into definition
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
            stackFrame.CreateLocalVariable($"{count}", stackFrame.ConsumeArgument());
            count++;
        }
        
        Console.WriteLine(string.Join(',', stackFrame.GetAllLocals()));
    }
}