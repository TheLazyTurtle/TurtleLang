namespace TurtleLang.Models.Ast;

class VariableAstNode: ValueAstNode
{
    public VariableAstNode(Token? token, BuildInTypes type) : base(Opcode.Variable, token, type)
    {
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