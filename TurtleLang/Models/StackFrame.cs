namespace TurtleLang.Models;

class StackFrame
{
    private readonly Queue<RuntimeValue> _functionArguments = new();
    private readonly Dictionary<string, RuntimeValue> _runtimeValueByName = new();

    public int ArgumentCount => _functionArguments.Count;

    public void AddArgument(RuntimeValue value)
    {
        _functionArguments.Enqueue(value);
    }

    public RuntimeValue GetArgument()
    {
        return _functionArguments.Dequeue();
    }

    public bool HasArguments()
    {
        return _functionArguments.Count > 0;
    }

    public void CreateLocalVariable(string variableName, RuntimeValue value)
    {
        _runtimeValueByName.Add(variableName, value);
    }

    public RuntimeValue? GetLocalVariableByName(string name)
    {
        if (!_runtimeValueByName.ContainsKey(name))
            return null;
        
        return _runtimeValueByName[name];
    }

    public IEnumerable<RuntimeValue> GetAllLocals()
    {
        return _runtimeValueByName.Values;
    }
}