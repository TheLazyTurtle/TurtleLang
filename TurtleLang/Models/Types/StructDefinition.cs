namespace TurtleLang.Models.Types;

class StructDefinition: TypeDefinition
{
    private readonly Dictionary<string, TypeDefinition> _definedFieldsByName = new();

    public StructDefinition(string name)
    {
        Name = name;
    }

    public void AddField(string fieldName, TypeDefinition type)
    {
        _definedFieldsByName.Add(fieldName, type);
    }

    public TypeDefinition GetFieldByName(string name)
    {
        if (_definedFieldsByName.TryGetValue(name, out var type)) 
            return type;
        
        InterpreterErrorLogger.LogError($"Field does not exist on struct {Name}");
        throw new Exception();
    }

    public bool ContainsField(string fieldName)
    {
        return _definedFieldsByName.ContainsKey(fieldName);
    }

    public Dictionary<string, TypeDefinition> Proxy_GetAllFields()
    {
        return _definedFieldsByName;
    }

    public int GetFieldCount()
    {
        return _definedFieldsByName.Count();
    }
}