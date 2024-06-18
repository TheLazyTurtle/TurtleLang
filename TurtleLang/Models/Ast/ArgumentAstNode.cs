using TurtleLang.Models.BuildIn.Types;

namespace TurtleLang.Models.Ast;

class ArgumentAstNode: AstNode
{
    public BuildInType Type;
    public ArgumentAstNode(Opcode opcode, Token? token, BuildInType type) : base(opcode, token)
    {
        Type = type;
    }
}