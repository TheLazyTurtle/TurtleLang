namespace TurtleLang.Models.Ast;

class ValueAstNode: AstNode
{
    public BuildInTypes Type;
    public ValueAstNode(Opcode opcode, Token? token, BuildInTypes type) : base(opcode, token)
    {
        Type = type;
    }

    public override string ToString()
    {
        if (Type == BuildInTypes.Int)
        {
            return $"{GetValueAsInt()}: Int";
        }

        return Type == BuildInTypes.String ? $"{GetValueAsString()}: String" : $"{GetValueAsString()}: Any";
    }
}