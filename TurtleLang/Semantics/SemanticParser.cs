using System.Diagnostics;
using TurtleLang.Models;
using TurtleLang.Models.Ast;
using TurtleLang.Models.Types;
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
        // TODO: Make a pass to validate that the return type of traits matches the return type of the impl.
        // TODO: Make a pass to validate that all return paths match the type of the return type.
        var success = ValidateAllTypesHaveDefinition();
        Debug.Assert(success);

        success = ValidateStructImplsHaveReturnType();
        Debug.Assert(success);

        success = ValidateTraitDefFunctionsHaveReturnType();
        Debug.Assert(success);
        
        success = ValidateAllAssignsHaveCorrectType();
        Debug.Assert(success);

        success = ValidateStructHasAllFieldsAssigned();
        Debug.Assert(success);

        success = ValidateStructImplementsAllTraitMethods();
        Debug.Assert(success);
        
        success = FunctionValidationPass();
        Debug.Assert(success);

        success = ValidateEveryFunctionHasReturnType();
        Debug.Assert(success);
        
        success = ValidateArguments();
        Debug.Assert(success);
    }

    private bool ValidateTraitDefFunctionsHaveReturnType()
    {
        foreach (var traitDefinition in TraitDefinitions.Proxy_GetAllTraits())
        {
            foreach (var functionDefinition in traitDefinition.Value.GetAllFunctions())
            {
                if (functionDefinition.Value.ReturnType == null)
                    InterpreterErrorLogger.LogError($"Function {functionDefinition.Key} defined in Trait {traitDefinition.Key} does not have a return type.");
            }
        }
        
        return true;
    }

    private bool ValidateStructImplsHaveReturnType()
    {
        foreach (var definition in TypeDefinitions.Proxy_GetAllTypeDefinitions())
        {
            foreach (var functionDefinition in definition.Value.Proxy_GetAllImplementedFunctions())
            {
                if (functionDefinition.Value is not FunctionDefinitionAstNode funcDef)
                    return false;

                if (funcDef.ReturnType == null)
                    InterpreterErrorLogger.LogError($"Function {functionDefinition.Key.Name} implemented for {definition.Key} does not have a return type");
            }
        }

        return true;
    }

    private bool ValidateEveryFunctionHasReturnType()
    {
        foreach (var functionDefinition in FunctionDefinitions.GetAll())
        {
            // TODO: Write handling for this
            if (functionDefinition.Value is BuildInFunctionAstNode)
                continue;
            
            if (functionDefinition.Value is not FunctionDefinitionAstNode funcDef)
                return false;

            if (funcDef.ReturnType == null)
            {
                InterpreterErrorLogger.LogError($"Function {funcDef.GetValueAsString()} does not have a return type");
                return false;
            }
        }

        return true;
    }

    private bool ValidateStructImplementsAllTraitMethods()
    {
        foreach (var typeDefinition in TypeDefinitions.GetAll())
        {
            if (typeDefinition.Value == null)
                continue;
            
            foreach (var traitDef in typeDefinition.Value.GetAllImplementedTraits())
            {
                foreach (var functionDefinition in traitDef.GetAllFunctions())
                {
                    if (typeDefinition.Value.ContainsFunction(functionDefinition.Key)) 
                        continue;
                    
                    InterpreterErrorLogger.LogError($"Function {functionDefinition.Key} from trait {traitDef.Name} not implemented on type {typeDefinition.Key}.");
                    return false;
                }
            }
        }
        return true;
    }

    private bool ValidateStructHasAllFieldsAssigned()
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

            if (current is not NewAstNode newAstNode) 
                continue;
            
            var definition = newAstNode.Type;
            if (definition is not StructDefinition structDefinition)
                throw new Exception("Handle adding base types to heap");

            if (newAstNode.GetAssignedValueCount() != structDefinition.GetFieldCount())
            {
                InterpreterErrorLogger.LogError($"Struct is missing fields.", newAstNode);
                return false;
            }

            foreach (var assignedValue in newAstNode.GetAssignedValues())
            {
                var assignedVar = assignedValue.Key;
                if (!structDefinition.ContainsField(assignedVar))
                {
                    InterpreterErrorLogger.LogError($"Tried to assign field that does not exist on struct. Field assigned: {assignedVar}", newAstNode);
                    return false;
                }
            }
        }

        return true;
    }

    private bool ValidateAllTypesHaveDefinition()
    {
        foreach (var typeDefinition in TypeDefinitions.Proxy_GetAllTypeDefinitions())
        {
            if (typeDefinition.Value != null) 
                continue;
            
            InterpreterErrorLogger.LogError($"Struct: {typeDefinition.Key} does not have a definition");
            return false;
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

            if (current is NewAstNode newAstNode)
            {
                var definition = newAstNode.Type;
                if (definition is not StructDefinition structDefinition)
                    throw new Exception("Handle adding base types to heap");
                
                foreach (var assignedValue in newAstNode.GetAssignedValues())
                {
                    var expectedType = structDefinition.GetFieldByName(assignedValue.Key);
                    var actualType = assignedValue.Value.Type;

                    if (expectedType.Equals(actualType)) 
                        continue;
                    
                    InterpreterErrorLogger.LogError($"Type was not expected. Expected {expectedType} got {actualType}.", newAstNode);
                    return false;
                }
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

            if (leftNode.Type.Equals(rightNode.Type)) 
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

                AstNode? astNode;
                if (child is CallMethodAstNode methodCallAstNode)
                {
                    var structDefinition = TypeDefinitions.GetByName(methodCallAstNode.NameOfStruct);
                    Debug.Assert(structDefinition != null, "If this is null we need more checks");
                    astNode = structDefinition.GetFunctionByName(methodCallAstNode.GetValueAsString());
                }
                else
                {
                    astNode = FunctionDefinitions.Get(child.GetValueAsString()!);
                }

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
            
            if (valueNode.Type.Equals(expectedType))
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
            
            if (valueNode.Type.Equals(expectedType))
                continue;
                
            return false;
        }
        
        return true;
    }
}