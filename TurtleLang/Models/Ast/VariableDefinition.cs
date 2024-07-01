namespace TurtleLang.Models.Ast;

class VariableDefinition
{
    public string Name { get; }
    public BuildInTypes Type { get; }
    public VariableAstNode VariableAstNode { get; private set; }

    public VariableDefinition(VariableAstNode variableAstNode)
    {
        Name = variableAstNode.GetValueAsString()!;
        Type = variableAstNode.Type;
        VariableAstNode = variableAstNode;
    }

    public override string ToString()
    {
        return $"{Name}: {Type}";
    }
}