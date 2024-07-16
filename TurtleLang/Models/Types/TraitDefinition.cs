namespace TurtleLang.Models.Types;

class TraitDefinition
{
    public string Name { get; }
    private readonly Dictionary<string, FunctionDefinition> _functionDefinitions = new();

    public TraitDefinition(string name)
    {
        Name = name;
    }

    public void AddFunctionDefinition(FunctionDefinition functionDefinition)
    {
        if (_functionDefinitions.ContainsKey(functionDefinition.Name))
        {
            InterpreterErrorLogger.LogError($"Tried to redefine function {functionDefinition.Name} for Trait {Name}.");
            return; 
        }
        
        _functionDefinitions.Add(functionDefinition.Name, functionDefinition);
    }

    public FunctionDefinition GetFunctionDefinitionByName(string name)
    {
        if (!_functionDefinitions.ContainsKey(name))
        {
            InterpreterErrorLogger.LogError($"Function {name} did not exist in trait {Name}");
            throw new Exception();
        }
        
        return _functionDefinitions[name];
    }

    public Dictionary<string, FunctionDefinition> GetAllFunctions()
    {
        return _functionDefinitions;
    }
}