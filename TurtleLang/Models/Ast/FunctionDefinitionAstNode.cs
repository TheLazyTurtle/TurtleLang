namespace TurtleLang.Models.Ast;

class FunctionDefinitionAstNode: AstNode
{
    public int ArgumentCount { get; private set; }
    public FunctionDefinitionAstNode(Opcode opcode, int lineNumber) : base(opcode, lineNumber)
    {
    }

    public FunctionDefinitionAstNode(Opcode opcode, string value, int lineNumber) : base(opcode, value, lineNumber)
    {
    }

    public void AddArgument()
    {
        ArgumentCount++;
    }
}