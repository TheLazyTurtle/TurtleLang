using System.Diagnostics;
using TurtleLang.Models.Types;

namespace TurtleLang.Repositories;

static class TraitDefinitions
{
    private static readonly Dictionary<string, TraitDefinition?> TraitDefinitionsByName = new();

    public static TraitDefinition? GetByName(string name)
    {
        Debug.Assert(TraitDefinitionsByName.ContainsKey(name));
        return TraitDefinitionsByName[name];
    }

    public static void AddOrDefine(string name, TraitDefinition? traitDefinition)
    {
        if (TraitDefinitionsByName.ContainsKey(name) && TraitDefinitionsByName[name] != null)
        {
            if (traitDefinition == null) 
                return;
            
            InterpreterErrorLogger.LogError($"Trying to redefine trait: {name}");
            return;
        }
        
        if (TraitDefinitionsByName.ContainsKey(name) && TraitDefinitionsByName[name] == null)
        {
            TraitDefinitionsByName[name] = traitDefinition;
            return;
        }
        
        TraitDefinitionsByName.Add(name, traitDefinition);
    }
}