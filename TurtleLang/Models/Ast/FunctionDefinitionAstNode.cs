﻿using System.Diagnostics;
using System.Text;

namespace TurtleLang.Models.Ast;

class VariableDefinition
{
    public string Name { get; }
    public BuildInTypes Type { get; }

    public VariableDefinition(string name, BuildInTypes type)
    {
        Name = name;
        Type = type;
    }

    public override string ToString()
    {
        return $"{Name}: {Type}";
    }
}

class FunctionDefinitionAstNode: ScopeableAstNode
{
    public List<VariableDefinition>? Arguments { get; set; }
    public int ArgumentCount => Arguments?.Count ?? 0;
    
    public FunctionDefinitionAstNode(Opcode opcode, Token? token) : base(opcode, token)
    {
    }

    public void AddArgument(VariableDefinition value)
    {
        Arguments ??= new List<VariableDefinition>();
        Arguments.Add(value);
    }
    
    public BuildInTypes GetTypeOfArgumentOnIndex(int index)
    {
        return Arguments[index].Type;
    }

    public VariableDefinition? GetArgumentByName(string name)
    {
        return Arguments?.FirstOrDefault(x => x.Name == name);
    }
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=========================");
        sb.Append(GetValueAsString());

        if (Arguments != null)
            sb.AppendLine($"({string.Join(',', Arguments)})");
        else
            sb.AppendLine("()");

        if (Children.Count == 0) 
            return sb.ToString();
        
        sb.AppendLine("{");
        foreach (var child in Children)
        {
            if (child is IfAstNode ifNode)
                sb.AppendLine($"{ifNode.ToString(1)}");
            else if (child is ForAstNode forAstNode)
                sb.AppendLine($"{forAstNode.ToString(1)}");
            else if (child is ExpressionAstNode expressionAstNode)
                sb.AppendLine($"{expressionAstNode.ToString(1)}");
            else
                sb.Append(child.ToString(1));
        }

        sb.AppendLine("}");

        return sb.ToString();
    }
}