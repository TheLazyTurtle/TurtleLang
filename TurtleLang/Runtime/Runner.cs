using System.Diagnostics;
using TurtleLang.Models;
using TurtleLang.Models.Ast;
using TurtleLang.Repositories;
using StackFrame = TurtleLang.Models.StackFrame;

namespace TurtleLang.Runtime;

class Runner
{
    private readonly Stack<StackFrame> _stack = new();
    private StackFrame? _stackFrameBeingBuild;
    
    public void Run(AstTree ast)
    {
        var callMain = ast.Children.FirstOrDefault();
        Debug.Assert(callMain != null);
        Debug.Assert(callMain.GetValueAsString() == "Main");
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
            case Opcode.If:
                HandleIfStatement(node);
                return;
            case Opcode.Else:
                Debug.Assert(false, "This should not happen as else should be handled by if");
                return;
            case Opcode.PushStackFrame:
                _stack.Push(_stackFrameBeingBuild!);
                InternalLogger.Log("Pushed stackframe");
                _stackFrameBeingBuild = null;
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
            var stackFrame = _stack.Peek();
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
    
    private void HandleLoadArgument(AstNode node)
    {
        var stackFrame = _stack.Peek();
        if (!stackFrame.HasArguments())
            return;
        
        var argumentName = node.GetValueAsString();
        stackFrame.CreateLocalVariable(argumentName, stackFrame.GetArgument());
        
        InternalLogger.Log($"Method now has access to argument: {argumentName} with value: {stackFrame.GetLocalVariableByName(argumentName)}");
    }

    private void HandleAddArgument(AstNode node)
    {
        _stackFrameBeingBuild ??= new StackFrame();
        
        // It is an int it can never be an variable
        var intValue = node.GetValueAsInt();
        if (intValue != null)
        {
            _stackFrameBeingBuild.AddArgument(new RuntimeValue(BuildInTypes.Int, intValue));
            InternalLogger.Log($"Adding int value of: {node.GetValueAsInt()} to stackframe");
            return;
        }

        // Handle string as input
        if (node is ValueAstNode argNode)
        {
            _stackFrameBeingBuild.AddArgument(new RuntimeValue(BuildInTypes.String, argNode.GetValueAsString()!));
            
            InternalLogger.Log($"Adding string value of: {argNode.GetValueAsString()} to stackframe");
            return;
        }
        
        // Handle passing a variable
        var currentFunctionStackFrame = _stack.Peek();

        // The current functions stackframe contains a local with this name so we can use this value.
        var localVariable = currentFunctionStackFrame.GetLocalVariableByName(node.GetValueAsString()!);
        if (localVariable == null)
            InterpreterErrorLogger.LogError($"Variable {node.GetValueAsString()} does not exist", node);
        
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

        var functionDefinition = FunctionDefinitions.Get(node.GetValueAsString());

        if (functionDefinition is FunctionDefinitionAstNode functionNode)
        {
            Debug.Assert(functionNode != null);
            
            if (stackFrame.ArgumentCount > functionNode.ArgumentCount)
                InterpreterErrorLogger.LogError("To many arguments given for function", node);
            
            if (stackFrame.ArgumentCount < functionNode.ArgumentCount)
                InterpreterErrorLogger.LogError("To few arguments given for function", node);
            
            InternalLogger.Log($"Executing function: {node.GetValueAsString()}");
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