using TurtleLang.Models.Ast;

namespace TurtleLang.Models.Types;

class TypeDefinition: IEquatable<TypeDefinition>
{
    public string Name { get; init; }
    private readonly Dictionary<FunctionDefinition, AstNode?> _functions = new();
    private readonly List<TraitDefinition> _traitDefinitions = new();

    public void AddFunctionDefinition(FunctionDefinition functionDefinition)
    {
        if (_functions.ContainsKey(functionDefinition))
        {
            InterpreterErrorLogger.LogError($"Trying to redefine function {functionDefinition.Name} in type {Name}");
            return;
        }
        
        _functions.Add(functionDefinition, null);
    }

    public bool ContainsFunction(string name)
    {
        return _functions.Any(x => x.Key.Name == name);
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

    public void AddTraitDefinition(TraitDefinition traitDefinition)
    {
        if (_traitDefinitions.Contains(traitDefinition))
        {
            InterpreterErrorLogger.LogError($"Tried to add {traitDefinition.Name} to type {Name}, but it already contains this trait");
            return;
        }
        
        _traitDefinitions.Add(traitDefinition);
    }

    public List<TraitDefinition> GetAllImplementedTraits()
    {
        return _traitDefinitions;
    }

    public AstNode? GetFunctionByName(string name)
    {
        var function = _functions.FirstOrDefault(x => x.Key.Name == name);
        return function.Value;
    }

    public Dictionary<FunctionDefinition, AstNode?> Proxy_GetAllImplementedFunctions()
    {
        return _functions;
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