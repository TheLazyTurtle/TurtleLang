using System.Text;

namespace TurtleLang.Models.Ast;

class AstNode
{
    public AstNode? Child { get; private set; }
    public AstNode? Sibling { get; private set; }
    public Opcode Opcode { get; }
    public string Value { get; }
    public int LineNumber { get; }

    public AstNode(Opcode opcode, int lineNumber)
    {
        Opcode = opcode;
        Value = "";
        LineNumber = lineNumber;
    }
    
    public AstNode(Opcode opcode, string value, int lineNumber)
    {
        Opcode = opcode;
        Value = value;
        LineNumber = lineNumber;
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
            sb.Append($"\n\t Child: -> {Child.ToString(0)}");

        if (Sibling != null)
            sb.Append($"\n| Sibling: {Sibling.ToString(0)}");

        return sb.ToString();
    }

    private string ToString(int depth)
    {
        var sb = new StringBuilder();

        sb.Append(Opcode.ToString());
        if (!string.IsNullOrEmpty(Value))
            sb.Append($": {Value}");

        depth++;
        if (Child != null)
            sb.Append($"\n\t Child: -> {Child.ToString(depth)}");

        if (Sibling != null)
            sb.Append($"\n| Sibling: {Sibling.ToString(depth)}");

        return sb.ToString();
    }
}