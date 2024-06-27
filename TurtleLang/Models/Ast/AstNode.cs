﻿using System.Text;

namespace TurtleLang.Models.Ast;

class AstNode
{
    public List<AstNode> Children { get; private set; } = new();
    public Opcode Opcode { get; }
    private readonly object _value;
    public int LineNumber { get; }

    public AstNode(Opcode opcode, Token? token)
    {
        Opcode = opcode;
        if (token == null)
        {
            _value = "";
            LineNumber = 0;
        }
        else
        {
            if (token.TokenType == TokenTypes.Int)
                _value = token.GetValueAsInt();
            else 
                _value = token.GetValueAsString();
            
            LineNumber = token.LineNumber;
        }
    }
    
    public void AddChild(AstNode node)
    {
        Children.Add(node);
    }

    public IEnumerable<AstNode> GetChildren()
    {
        return Children;
    }
    
    public override string ToString()
    {
        if (_value is string s)
            return $"{Opcode.ToString()} {s}";
        
        if (_value is int i)
            return $"{Opcode.ToString()} {i}";

        return Opcode.ToString();
    }

    public int? GetValueAsInt()
    {
        if (_value is int i)
            return i;

        return null;
    }

    public string? GetValueAsString()
    {
        if (_value is string s)
            return s;

        return null;
    }

    public object Proxy_GetRawValue()
    {
        return _value;
    }

    public string ToString(int depth)
    {
        var sb = new StringBuilder();
        var padding = new string(' ', depth * 2); // Double spaces

        if (_value is string s)
        {
            if (!string.IsNullOrEmpty(s))
                sb.AppendLine($"{padding}{Opcode.ToString()}: {GetValueAsString()}");
            else
                sb.AppendLine($"{padding}{Opcode.ToString()}");
        }
        else if (_value is int)
        {
            sb.AppendLine($"{padding}{Opcode.ToString()}: {GetValueAsInt()}");
        }
        else
        {
            sb.AppendLine($"{padding}{Opcode.ToString()}");
        }

        depth++;
        foreach (var child in Children)
        {
            if (child is IfAstNode ifNode)
                sb.AppendLine($"{ifNode.ToString(depth)}");
            else if (child is ForAstNode forAstNode)
                sb.AppendLine($"{forAstNode.ToString(depth)}");
            else if (child is ExpressionAstNode expressionAstNode)
                sb.AppendLine($"{expressionAstNode.ToString(depth)}");
            else
                sb.Append(child.ToString(depth));
        }

        return sb.ToString();
    }
}