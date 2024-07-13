using TurtleLang.Models.Ast;

namespace TurtleLang.Models.Types;

class TypeDefinition: IEquatable<TypeDefinition>
{
    public string Name { get; init; }
    private readonly Dictionary<FunctionDefinition, AstNode?> _functions = new();

    public void AddFunctionDefinition(FunctionDefinition functionDefinition)
    {
        if (_functions.ContainsKey(functionDefinition))
        {
            InterpreterErrorLogger.LogError($"Trying to redefine function {functionDefinition.Name} in type {Name}");
            return;
        }
        
        _functions.Add(functionDefinition, null);
    }

    public void AddFunction(FunctionDefinition functionDefinition, AstNode functionImpl)
    {
        if (_functions.ContainsKey(functionDefinition) && _functions[functionDefinition] != null)
        {
            InterpreterErrorLogger.LogError($"Trying to redefine function {functionDefinition.Name} in type {Name}");
            return;
        }
        
        _functions.Add(functionDefinition, functionImpl);
    }

    public AstNode? GetFunctionByName(string name)
    {
        var function = _functions.FirstOrDefault(x => x.Key.Name == name);
        return function.Value;
    }

    public bool Equals(TypeDefinition? other)
    {
        if (other == null)
            return false;
        
        return Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        throw new NotImplementedException();
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public override string ToString()
    {
        return Name;
    }
}