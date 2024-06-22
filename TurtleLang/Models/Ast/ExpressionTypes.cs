namespace TurtleLang.Models.Ast;

enum ExpressionTypes
{
    Increase,
    Decrease,
    Assign,
    Eq,
    Gt,
    Gte,
    Lt,
    Lte
}

static class ExpressionTypesExtensions
{
    public static string GetDisplayValue(this ExpressionTypes type)
    {
        return type switch
        {
            ExpressionTypes.Increase => "++",
            ExpressionTypes.Decrease => "--",
            ExpressionTypes.Assign => "=",
            ExpressionTypes.Eq => "==",
            ExpressionTypes.Gt => ">",
            ExpressionTypes.Gte => ">=",
            ExpressionTypes.Lt => "<",
            ExpressionTypes.Lte => "<=",
            var _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}