using System.Diagnostics;
using TurtleLang.Models.Types;

namespace TurtleLang.Models;

class RuntimeStruct
{
    public StructDefinition StructDefinition { get; private set; }
    private readonly Dictionary<string, RuntimeValue> _fieldsByName = new();
    
    public RuntimeStruct(StructDefinition structDefinition)
    {
        StructDefinition = structDefinition;

        foreach (var field in structDefinition.Proxy_GetAllFields())
        {
            _fieldsByName.Add(field.Key, new RuntimeValue(field.Value, null));
        }
    }

    public RuntimeValue GetVariableByRef(string variableName)
    {
        Debug.Assert(_fieldsByName.ContainsKey(variableName));
        return _fieldsByName[variableName];
    }
}