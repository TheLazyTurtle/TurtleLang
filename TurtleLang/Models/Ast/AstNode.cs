using System.Text;

namespace TurtleLang.Models.Ast;

class AstNode
{
    public List<AstNode> Children { get; private set; } = new();
    public Opcode Opcode { get; }
    public string Value { get; }
    public int LineNumber { get; }

    public AstNode(Opcode opcode, Token? token)
    {
        Opcode = opcode;
        if (token == null)
        {
            Value = "";
            LineNumber = 0;
        }
        else
        {
            Value = token.Value;
            LineNumber = token.LineNumber;
        }
    }
    
    public void AddChild(AstNode node)
    {
        Children.Add(node);
    }

    public override string ToString()
    {
        return $"{Opcode.ToString()} {Value}";
    }

    public string ToString(int depth)
    {
        var sb = new StringBuilder();
        var padding = new string(' ', depth * 2); // Double spaces

        if (!string.IsNullOrEmpty(Value))
            sb.AppendLine($"{padding}{Opcode.ToString()}: {Value}");
        else
            sb.AppendLine($"{padding}{Opcode.ToString()}");

        depth++;
        foreach (var child in Children)
        {
            sb.Append($"{child.ToString(depth)}");
            if (child.Opcode == Opcode.Return)
            {
                depth--;
            }
        }

        return sb.ToString();
    }
}