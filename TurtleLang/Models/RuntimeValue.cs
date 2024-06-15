namespace TurtleLang.Models;

class RuntimeValue
{
    public PrimitiveTypes Type { get; init; }
    public object Value { get; init; }

    public RuntimeValue(PrimitiveTypes type, object value)
    {
        Type = type;
        Value = value;
    }

    public override string ToString()
    {
        return Type switch
        {
            PrimitiveTypes.Int => $"{(int)Value}",
            PrimitiveTypes.String => ((string)Value).Replace("\"", ""),
            var _ => throw new ArgumentOutOfRangeException()
        };
    }
}