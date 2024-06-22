namespace TurtleLang.Models.Ast;

class ExpressionAstNode: AstNode
{
    public ValueAstNode Left { get; }
    public ValueAstNode Right { get; }
    public ExpressionTypes ExpressionType { get; }
    
    public ExpressionAstNode(ValueAstNode left, ValueAstNode right, ExpressionTypes expressionType, Token? token) : base(Opcode.Expression, token)
    {
        Left = left;
        Right = right;
        ExpressionType = expressionType;
    }

    public override string ToString()
    {
        return $"{Left} {ExpressionType.GetDisplayValue()} {Right}";
    }
}