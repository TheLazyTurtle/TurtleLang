using System.Text;

namespace TurtleLang.Models.Ast;

class AstTree
{
    public List<AstNode> Children { get; private set; } = new();

    public void AddChild(AstNode child)
    {
        Children.Add(child);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (var child in Children)
        {
            if (child is FunctionDefinitionAstNode funcDef)
                sb.AppendLine($"{funcDef.ToString()}");
            else 
                sb.AppendLine($"{child.ToString(0)}");
        }

        return sb.ToString();
    }
}