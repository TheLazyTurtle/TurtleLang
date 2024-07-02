using TurtleLang.Models.Types;

namespace TurtleLang.Models.Ast;

class ValueAstNode: AstNode
{
    public TypeDefinition Type;
    public ValueAstNode(Token? token, TypeDefinition type) : base(Opcode.Value, token)
    {
        Type = type;
    }
    
    protected ValueAstNode(Opcode opcode, Token? token, TypeDefinition type) : base(opcode, token)
    {
        Type = type;
    }

    public override string ToString()
    {
        if (Type is IntTypeDefinition)
        {
            return $"{GetValueAsInt()}: Int";
        }

        return Type is StringTypeDefinition ? $"{GetValueAsString()}: String" : $"{GetValueAsString()}: Any";
    }
}