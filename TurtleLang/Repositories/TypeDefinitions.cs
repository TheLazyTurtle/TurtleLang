using System.Collections.ObjectModel;
using System.Diagnostics;
using TurtleLang.Models.Types;

namespace TurtleLang.Repositories;

static class TypeDefinitions
{
    private static readonly Dictionary<string, TypeDefinition?> TypeDefinitionByName = new();

    static TypeDefinitions()
    {
        TypeDefinitionByName.Add("string", new StringTypeDefinition());
        TypeDefinitionByName.Add("i32", new IntTypeDefinition());
    }

    public static bool Contains(string name)
    {
        return TypeDefinitionByName.ContainsKey(name);
    }

    public static TypeDefinition? GetByName(string name)
    {
        Debug.Assert(TypeDefinitionByName.ContainsKey(name));
        return TypeDefinitionByName[name];
    }
    
    public static void AddOrDefine(string name, TypeDefinition? structDefinition)
    {
        if (TypeDefinitionByName.ContainsKey(name) && TypeDefinitionByName[name] != null)
        {
            if (structDefinition == null) 
                return;
            
            InterpreterErrorLogger.LogError($"Trying to redefine struct: {name}");
            return;
        }
        
        if (TypeDefinitionByName.ContainsKey(name) && TypeDefinitionByName[name] == null)
        {
            TypeDefinitionByName[name] = structDefinition;
            return;
        }
        
        TypeDefinitionByName.Add(name, structDefinition);
    }

    public static Dictionary<string, TypeDefinition?> GetAll()
    {
        return TypeDefinitionByName;
    }

    public static IReadOnlyDictionary<string, TypeDefinition?> Proxy_GetAllTypeDefinitions()
    {
        return TypeDefinitionByName;
    }
}