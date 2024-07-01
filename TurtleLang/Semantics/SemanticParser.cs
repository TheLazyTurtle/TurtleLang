using System.Diagnostics;
using System.Text;
using TurtleLang.Models;
using TurtleLang.Models.Ast;
using TurtleLang.Repositories;

namespace TurtleLang.Semantics;

class SemanticParser
{
    private readonly AstTree _ast;
    public SemanticParser(AstTree ast)
    {
        _ast = ast;
    }

    public void Validate()
    {
        var success = false;
        success = ValidateAllVarsHaveTypes();
        Debug.Assert(success);
        success = ValidateAllAssignsHaveCorrectType();
        Debug.Assert(success);
        success = FunctionValidationPass();
        Debug.Assert(success);
        success = ValidateArguments();
        Debug.Assert(success);
    }
    
    private bool ValidateAllVarsHaveTypes()
    {
        var todo = new Queue<AstNode>();
        
        foreach (var astNode in FunctionDefinitions.GetAll())
        {
            if (astNode.Value == null)
                continue;
            
            todo.Enqueue(astNode.Value);
        }

        while (todo.Count > 0)
        {
            var current = todo.Dequeue();

            if (current is VariableAstNode { Type: BuildInTypes.Any })
                throw new Exception("Type is any");

            foreach (var child in current.Children)
            {
                todo.Enqueue(child);
            }
        }

        return true;
    }

    private bool ValidateAllAssignsHaveCorrectType()
    {
        var todo = new Queue<AstNode>();
        
        foreach (var astNode in FunctionDefinitions.GetAll())
        {
            if (astNode.Value == null)
                continue;
            
            todo.Enqueue(astNode.Value);
        }

        while (todo.Count > 0)
        {
            var current = todo.Dequeue();
            
            foreach (var child in current.Children)
            {
                todo.Enqueue(child);
            }

            if (current is not ExpressionAstNode expressionNode)
                continue;

            var left = expressionNode.Left;
            var right = expressionNode.Right;

            if (left is not VariableAstNode leftNode)
            {
                Debug.Assert(false);
                return false;
            }

            if (right is not ValueAstNode rightNode)
            {
                Debug.Assert(false);
                return false;
            }

            if (leftNode.Type == rightNode.Type) 
                continue;
            
            InterpreterErrorLogger.LogError("Type assigned does not match type for variable", expressionNode);
            return false;
        }

        return true;
    }
    
    // Validates that every called function has an impl
    private bool FunctionValidationPass()
    {
        var result = true;
        
        var functionDefinitions = FunctionDefinitions.GetAll();
        foreach (var functionDefinition in functionDefinitions)
        {
            if (functionDefinition.Value != null) 
                continue;
            
            result = false;
            InterpreterErrorLogger.LogError($"Function: {functionDefinition.Key} get called but is never defined");
        }

        return result;
    }

    private bool ValidateArguments()
    {
        var result = true;

        foreach (var functionDefinition in FunctionDefinitions.GetAll())
        {
            if (functionDefinition.Value is not FunctionDefinitionAstNode funcDef)
                continue;

            foreach (var child in funcDef.Children)
            {
                if (child.Opcode != Opcode.Call)
                    continue;

                var astNode = FunctionDefinitions.Get(child.GetValueAsString()!);
                if (astNode is FunctionDefinitionAstNode functionDefinitionToValidate)
                {
                    Debug.Assert(functionDefinitionToValidate != null);
                    if (child.Children.Count != functionDefinitionToValidate.ArgumentCount)
                    {
                        InterpreterErrorLogger.LogError($"Argument count does not match for: {functionDefinitionToValidate.GetValueAsString()}. Expected {functionDefinitionToValidate.ArgumentCount}, got: {child.Children.Count}");
                        result = false;
                    }

                    if (!DoesArgumentTypesMatch(child, functionDefinitionToValidate))
                        result = false;
                    
                    continue;
                }
                
                // Build in functions
                if (astNode is not BuildInFunctionAstNode buildInFunction)
                    continue;
                
                Debug.Assert(buildInFunction != null);
                if (!buildInFunction.InfiniteAmountOfParameters && child.Children.Count != buildInFunction.ArgumentCount)
                {
                    InterpreterErrorLogger.LogError($"Argument count does not match for: {buildInFunction.Name}. Expected {buildInFunction.ArgumentCount}, got: {child.Children.Count}");
                    result = false;
                }

                if (!DoesArgumentTypesMatch(child, buildInFunction))
                {
                    result = false;
                }
            }
        }

        return result;
    }

    private bool DoesArgumentTypesMatch(AstNode callingNode, FunctionDefinitionAstNode functionDefinition)
    {
        if (functionDefinition.ArgumentCount == 0)
            return true;
        
        for (var i = 0; i < callingNode.Children.Count; i++)
        {
            var passedArgument = callingNode.Children[i];
            var expectedType = functionDefinition.GetTypeOfArgumentOnIndex(i);

            if (passedArgument is not ValueAstNode valueNode) 
                continue;
            
            if (valueNode.Type == expectedType)
                continue;
                
            InterpreterErrorLogger.LogError($"Types for arguments passed into function {functionDefinition.GetValueAsString()} do not match. Expected {expectedType}, got {valueNode.Type}");
            return false;
        }

        return true;
    }
    
    private bool DoesArgumentTypesMatch(AstNode callingNode, BuildInFunctionAstNode functionDefinition)
    {
        if (functionDefinition.ArgumentCount == 0 || functionDefinition.InfiniteAmountOfParameters)
            return true;
        
        for (var i = 0; i < callingNode.Children.Count; i++)
        {
            var passedArgument = callingNode.Children[i];
            var expectedType = functionDefinition.GetTypeOfArgumentOnIndex(i);

            if (passedArgument is not ValueAstNode valueNode) 
                continue;
            
            if (valueNode.Type == expectedType)
                continue;
                
            return false;
        }
        
        return true;
    }
}