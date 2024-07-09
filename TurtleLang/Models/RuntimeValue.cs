using System.Diagnostics;
using TurtleLang.Models.Ast;
using TurtleLang.Models.Types;

namespace TurtleLang.Models;

class RuntimeValue
{
    public TypeDefinition Type { get; init; }
    public object? Value { get; private set; }

    public RuntimeValue(TypeDefinition type, object value)
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

        if (Type is IntTypeDefinition)
            return $"{(int)Value}";

        if (Type is StringTypeDefinition)
            return (string)Value;

        if (Type is StructDefinition)
            return $"addr: {(int)Value}"; // This will print the mem addr

        throw new ArgumentOutOfRangeException();
    }
}