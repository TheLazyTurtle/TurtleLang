namespace TurtleLang.Models;

class StackFrame
{
    private readonly Queue<RuntimeValue> _functionArguments = new();
    private readonly Dictionary<string, RuntimeValue> _localsByName = new();
    public required StackFrameTypes StackFrameType { get; init; }

    public int ArgumentCount => _functionArguments.Count;

    public void AddArgument(RuntimeValue value)
    {
        _functionArguments.Enqueue(value);
    }

    public RuntimeValue ConsumeArgument()
    {
        return _functionArguments.Dequeue();
    }

    public bool HasArguments()
    {
        return _functionArguments.Count > 0;
    }

    public void CreateLocalVariable(string variableName, RuntimeValue value)
    {
        _localsByName.Add(variableName, value);
    }

    public RuntimeValue? GetLocalVariableByName(string name)
    {
        if (!_localsByName.ContainsKey(name))
            return null;
        
        return _localsByName[name];
    }

    public IEnumerable<RuntimeValue> GetAllLocals()
    {
        return _localsByName.Values;
    }
}