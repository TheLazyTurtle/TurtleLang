using System.Text;

namespace TurtleLang.Models.Ast;

class FunctionDefinitionAstNode: AstNode
{
    public List<string>? Arguments { get; set; }
    public int ArgumentCount => Arguments?.Count ?? 0;
    
    public FunctionDefinitionAstNode(Opcode opcode, Token? token) : base(opcode, token)
    {
    }

    public void AddArgument(string identifier)
    {
        Arguments ??= new List<string>();
        Arguments.Add(identifier);
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

        foreach (var child in Children)
            sb.Append(child.ToString(1));

        return sb.ToString();
    }
}