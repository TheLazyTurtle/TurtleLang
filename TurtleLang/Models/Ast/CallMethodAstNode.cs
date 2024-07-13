namespace TurtleLang.Models.Ast;

class CallMethodAstNode: AstNode
{
    public string NameOfStruct { get; }
    public CallMethodAstNode(string nameOfStruct, Token? token) : base(Opcode.Call, token)
    {
        NameOfStruct = nameOfStruct;
    }
}