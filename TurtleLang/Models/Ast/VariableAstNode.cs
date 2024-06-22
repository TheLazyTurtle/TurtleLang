namespace TurtleLang.Models.Ast;

class VariableAstNode: ValueAstNode
{
    public VariableAstNode(Token? token) : base(Opcode.Variable, token, BuildInTypes.Any)
    {
    }
}