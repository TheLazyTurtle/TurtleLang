using TurtleLang.Models.Types;

namespace TurtleLang.Models;

class FunctionDefinition
{
    public string Name { get; }
    private readonly Dictionary<string, TypeDefinition> _arguments = new();

    public FunctionDefinition(string name)
    {
        Name = name;
    }

    public void AddArgument(string name, TypeDefinition typeDef)
    {
        if (_arguments.ContainsKey(name))
        {
            InterpreterErrorLogger.LogError($"Trying to redefine argument {name} {typeDef} for function {Name}");
            return;
        }
        
        _arguments.Add(name, typeDef);
    }

    public TypeDefinition GetArgumentByName(string name)
    {
        if (!_arguments.ContainsKey(name))
            throw new Exception($"Trying to access argument {name} for function {Name}");

        return _arguments[name];
    }
}