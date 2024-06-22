using System.Text;

namespace TurtleLang.Models.Ast;

class AstNode
{
    public Opcode Opcode { get; }
    private object _value;
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

        return sb.ToString();
    }
}