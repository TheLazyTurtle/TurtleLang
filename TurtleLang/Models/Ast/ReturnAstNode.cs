namespace TurtleLang.Models.Ast;

class ReturnAstNode: AstNode
{
    public ValueAstNode? ReturnValue { get; }
    
    public ReturnAstNode(Token? token, ValueAstNode? returnValue) : base(Opcode.Return, token)
    {
        ReturnValue = returnValue;
    }
}