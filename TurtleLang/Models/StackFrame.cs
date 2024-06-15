namespace TurtleLang.Models;

class StackFrame
{
    private readonly Queue<RuntimeValue> _functionArguments = new();
    private readonly Dictionary<string, RuntimeValue> _runtimeValueByName = new();

    public void AddArgument(RuntimeValue value)
    {
        _functionArguments.Enqueue(value);
    }

    public RuntimeValue GetArgument()
    {
        return _functionArguments.Dequeue();
    }

    public void CreateLocalVariable(string variableName, RuntimeValue value)
    {
        _runtimeValueByName.Add(variableName, value);
    }
}