﻿namespace TurtleLang.Models.Types;

class StructDefinitionAstNode: TypeDefinition
{
    private readonly Dictionary<string, TypeDefinition> _fieldsByName = new();

    public StructDefinitionAstNode(string name)
    {
        Name = name;
    }

    public void AddField(string fieldName, TypeDefinition type)
    {
        _fieldsByName.Add(fieldName, type);
    }

    public TypeDefinition GetFieldByName(string name)
    {
        if (_fieldsByName.TryGetValue(name, out var type)) 
            return type;
        
        InterpreterErrorLogger.LogError("Field does not exist on struct");
        throw new Exception();
    }
}