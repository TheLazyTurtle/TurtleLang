using TurtleLang.Models.Types;

namespace TurtleLang.Models.Ast;

class VariableAstNode: ValueAstNode
{
    public VariableAstNode(Token? token, TypeDefinition type) : base(Opcode.Variable, token, type)
    {
    }
    
    public override string ToString()
    {
        if (Type is IntTypeDefinition)
        {
            return $"{GetValueAsString()}: Int";
        }

        return Type is StringTypeDefinition ? $"{GetValueAsString()}: String" : $"{GetValueAsString()}: Any";
    }
}