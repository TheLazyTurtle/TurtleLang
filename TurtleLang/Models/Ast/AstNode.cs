﻿using System.Text;

namespace TurtleLang.Models.Ast;

class AstNode
{
    public AstNode? Child { get; private set; }
    public AstNode? Sibling { get; private set; }
    public Opcode Opcode { get; init; }
    public string Value { get; init; }

    public AstNode(Opcode opcode)
    {
        Opcode = opcode;
        Value = "";
    }
    
    public AstNode(Opcode opcode, string value)
    {
        Opcode = opcode;
        Value = value;
    }

    public void AddChild(AstNode node)
    {
        if (Child != null)
        {
            Child.AddSibling(node);
            return;
        }

        Child = node;
    }

    public void AddSibling(AstNode node)
    {
        if (Sibling != null)
        {
            Sibling.AddSibling(node);
            return;
        }

        Sibling = node;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append(Opcode.ToString());
        if (!string.IsNullOrEmpty(Value))
            sb.Append($": {Value}");

        if (Child != null)
            sb.Append($"\n\t Child: -> {Child}");

        if (Sibling != null)
            sb.Append($"\n| Sibling: {Sibling}");

        return sb.ToString();
    }
}