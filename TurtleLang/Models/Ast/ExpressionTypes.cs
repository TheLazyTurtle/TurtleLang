namespace TurtleLang.Models.Ast;

enum ExpressionTypes
{
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
            ExpressionTypes.Eq => "==",
            ExpressionTypes.Gt => ">",
            ExpressionTypes.Gte => ">=",
            ExpressionTypes.Lt => "<",
            ExpressionTypes.Lte => "<=",
            var _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}