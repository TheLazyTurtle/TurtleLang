using TurtleLang.Models.Scopes;

namespace TurtleLang.Models.Ast;

class ScopeableAstNode : AstNode
{
    public BaseScope? Scope { get; protected set; }
    public ScopeableAstNode(Opcode opcode, Token? token) : base(opcode, token)
    {
    }
    
    public void AddChild(AstNode node)
    {
        if (Scope == null)
        {
            InterpreterErrorLogger.LogError("Scope was null");
            return;
        }
        
        Scope.AddChild(node);
    }

    public IEnumerable<AstNode>? GetChildren()
    {
        return Scope?.Children;
    }
}