namespace TurtleLang.Models.Ast;

class ScopeableAstNode : AstNode
{
    public List<AstNode> Children { get; private set; } = new();
    public ScopeableAstNode(Opcode opcode, Token? token) : base(opcode, token)
    {
    }
    
    public void AddChild(AstNode node)
    {
        Children.Add(node);
    }

    public IEnumerable<AstNode> GetChildren()
    {
        return Children;
    }
}