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
        return $"{Left} {ExpressionType.GetDisplayValue()} {Right}";
    }
}