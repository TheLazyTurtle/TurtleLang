using System.Diagnostics;
using System.Text;

namespace TurtleLang.Models.Ast;

class ExpressionAstNode: AstNode
{
    public AstNode Left { get; }
    public AstNode? Right { get; }
    public ExpressionTypes ExpressionType { get; }
    
    public ExpressionAstNode(AstNode left, AstNode right, ExpressionTypes expressionType, Token? token) : base(Opcode.Expression, token)
    {
        Debug.Assert(left is ValueAstNode or VariableAstNode);
        Debug.Assert(right is ValueAstNode or VariableAstNode);
        
        Left = left;
        Right = right;
        ExpressionType = expressionType;
    }
    
    public ExpressionAstNode(AstNode left, ExpressionTypes expressionType, Token? token) : base(Opcode.Expression, token)
    {
        Debug.Assert(left is ValueAstNode or VariableAstNode);
        
        Left = left;
        ExpressionType = expressionType;
    }

    public new string ToString(int depth)
    {
        var sb = new StringBuilder();
        var padding = new string(' ', depth * 2); // Double spaces

        sb.Append($"{padding}{ToString()}");

        return sb.ToString();
    }

    public override string ToString()
    {
        var leftStr = Left.ToString();
        var rightStr = Right?.ToString() ?? "";

        if (Left is VariableAstNode lVariableAstNode)
            leftStr = lVariableAstNode.ToString();
        else if (Left is ValueAstNode lValueAstNode)
            leftStr = lValueAstNode.ToString();
        
        if (Right is VariableAstNode rVariableAstNode)
            rightStr = rVariableAstNode.ToString();
        else if (Right is ValueAstNode rValueAstNode)
            rightStr = rValueAstNode.ToString();

        return $"{leftStr} {ExpressionType.GetDisplayValue()} {rightStr}";
    }
}