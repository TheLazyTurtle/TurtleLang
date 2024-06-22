using TurtleLang.Models.Ast;

namespace TurtleLang.Models.Scopes;

abstract class BaseScope
{
    public List<AstNode> Children { get; } = new();
    
    public void AddChild(AstNode node)
    {
        Children.Add(node);
    }
}