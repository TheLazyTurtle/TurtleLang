using System.Diagnostics;

namespace TurtleLang.Models.Ast;

class ScopeableAstNode : AstNode
{
    public ScopeableAstNode(Opcode opcode, Token? token) : base(opcode, token)
    {
    }
}