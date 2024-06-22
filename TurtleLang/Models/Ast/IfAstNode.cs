using System.Text;
using TurtleLang.Models.Scopes;

namespace TurtleLang.Models.Ast;

class IfAstNode: ScopeableAstNode
{
    public ExpressionAstNode Expression { get; }
    
    public IfAstNode(ExpressionAstNode expression, Token? token) : base(Opcode.If, token)
    {
        Expression = expression;
    }

    public void AddScope(IfScope scope)
    {
        if (Scope != null)
            InterpreterErrorLogger.LogError("Function already had an scope");

        Scope = scope;
    }
    
    public new string ToString(int depth)
    {
        var sb = new StringBuilder();
        var padding = new string(' ', depth * 2); // Double spaces

        sb.Append($"{padding}if ({Expression}) ");

        depth++;
        if (Scope == null)
            return sb.ToString();

        sb.Append($"\n{padding}");
        sb.AppendLine("{");
        
        foreach (var child in Scope.Children)
        {
            sb.Append($"{child.ToString(depth)}");
            if (child.Opcode is Opcode.Return)
                depth--;
        }

        sb.Append($"{padding}");
        sb.AppendLine("}");

        return sb.ToString();
    }
}