using System.Diagnostics;

namespace TurtleLang.Models.Ast;

class ExpressionAstNode: AstNode
{
    public AstNode Left { get; }
    public AstNode Right { get; }
    public ExpressionTypes ExpressionType { get; }
    
    public ExpressionAstNode(AstNode left, AstNode right, ExpressionTypes expressionType, Token? token) : base(Opcode.Expression, token)
    {
        Debug.Assert(left is ValueAstNode or VariableAstNode);
        Debug.Assert(right is ValueAstNode or VariableAstNode);
        
        Left = left;
        Right = right;
        ExpressionType = expressionType;
    }

    public override string ToString()
    {
        return $"{Left} {ExpressionType.GetDisplayValue()} {Right}";
    }
}