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
}