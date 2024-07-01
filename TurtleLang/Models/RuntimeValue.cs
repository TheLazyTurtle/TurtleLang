using System.Diagnostics;
using TurtleLang.Models.Ast;

namespace TurtleLang.Models;

class RuntimeValue
{
    public BuildInTypes Type { get; init; }
    public object? Value { get; private set; }

    public RuntimeValue(BuildInTypes type, object value)
    {
        Type = type;
        Value = value;
    }

    public int GetValueAsInt()
    {
        Debug.Assert(Value != null);
        return (int)Value;
    }

    public string GetValueAsString()
    {
        Debug.Assert(Value != null);
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

    public void Proxy_SetRawValue(object value)
    {
        Value = value;
    }

    public override string ToString()
    {
        if (Value == null)
            return $"Uninitialized {Type}";
        
        return Type switch
        {
            BuildInTypes.Int => $"{(int)Value}",
            BuildInTypes.String => (string)Value,
            var _ => throw new ArgumentOutOfRangeException()
        };
    }
}