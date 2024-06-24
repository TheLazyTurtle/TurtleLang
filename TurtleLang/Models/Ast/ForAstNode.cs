using System.Text;

namespace TurtleLang.Models.Ast;

class ForAstNode: ScopeableAstNode
{
    public ExpressionAstNode? Expression { get; private set; }
    public ExpressionAstNode? IncrementExpression { get; private set; }
    public ExpressionAstNode? Initializer { get; private set; }
    
    public ForAstNode(Token? token) : base(Opcode.For, token)
    {
    }

    public void AddExpression(ExpressionAstNode expression)
    {
        if (Expression != null)
        {
            InterpreterErrorLogger.LogError("Expression already set for for loop");
            return;
        }

        Expression = expression;
    }

    public void AddIncrementExpression(ExpressionAstNode expression)
    {
        if (IncrementExpression != null)
        {
            InterpreterErrorLogger.LogError("Increment expression already set for for loop");
            return;
        }

        IncrementExpression = expression;
    }

    public void AddInitializer(ExpressionAstNode initializer)
    {
        if (Initializer != null)
        {
            InterpreterErrorLogger.LogError("Initializer already set for for loop");
            return;
        }

        Initializer = initializer;
    }

    public new string ToString(int depth)
    {
        var sb = new StringBuilder();
        
        var padding = new string(' ', depth * 2); // Double spaces

        sb.Append($"{padding}for({Initializer}; {Expression}; {IncrementExpression}) ");
        
        depth++;

        sb.Append($"\n{padding}");
        sb.AppendLine("{");
        
        foreach (var child in Children)
        {
            if (child is IfAstNode ifNode)
                sb.AppendLine($"{ifNode.ToString(depth)}");
            else if (child is ForAstNode forAstNode)
                sb.AppendLine($"{forAstNode.ToString(depth)}");
            else if (child is ExpressionAstNode expressionAstNode)
                sb.AppendLine($"{expressionAstNode.ToString(depth)}");
            else
                sb.Append(child.ToString(depth));
            
            if (child.Opcode is Opcode.Return)
                depth--;
        }

        sb.Append($"{padding}");
        sb.AppendLine("}");

        return sb.ToString();
    }
}