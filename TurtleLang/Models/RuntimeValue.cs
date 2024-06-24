using TurtleLang.Models.Ast;

namespace TurtleLang.Models;

class RuntimeValue
{
    public BuildInTypes Type { get; init; }
    public object Value { get; private set; }

    public RuntimeValue(BuildInTypes type, object value)
    {
        Type = type;
        Value = value;
    }

    public int GetValueAsInt()
    {
        return (int)Value;
    }

    public string GetValueAsString()
    {
        return (string)Value;
    }

    public void SetValueAsInt(int newValue)
    {
        Value = newValue;
    }

    public void SetValueAsString(string newValue)
    {
        Value = newValue;
    }

    public override string ToString()
    {
        return Type switch
        {
            BuildInTypes.Int => $"{(int)Value}",
            BuildInTypes.String => (string)Value,
            var _ => throw new ArgumentOutOfRangeException()
        };
    }
}