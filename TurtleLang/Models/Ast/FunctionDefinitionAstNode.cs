namespace TurtleLang.Models.Ast;

class FunctionDefinitionAstNode: AstNode
{
    public int ArgumentCount { get; private set; }
    public FunctionDefinitionAstNode(Opcode opcode) : base(opcode)
    {
    }

    public FunctionDefinitionAstNode(Opcode opcode, string value) : base(opcode, value)
    {
    }

    public void AddArgument()
    {
        ArgumentCount++;
    }
}