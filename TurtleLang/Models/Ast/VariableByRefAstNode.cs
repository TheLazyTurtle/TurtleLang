using TurtleLang.Models.Types;

namespace TurtleLang.Models.Ast;

class VariableByRefAstNode: VariableAstNode
{
    public VariableAstNode StructVariable { get; }
    public string FieldNameToGetByRef { get; }
    public VariableByRefAstNode(Token? token, VariableAstNode structVariable, string fieldName, TypeDefinition typeOfField) : base(token, typeOfField)
    {
        StructVariable = structVariable;
        FieldNameToGetByRef = fieldName;
    }
}