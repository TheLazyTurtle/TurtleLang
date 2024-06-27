namespace TurtleLang.Repositories;

static class TypeDefinitions
{
    private static readonly List<string> TypeNames = new();
    
    public static bool Contains(string name)
    {
        return TypeNames.Contains(name);
    }

    public static void Add(string name)
    {
        TypeNames.Add(name);
    }
    
    public static void AddIfNotExists(string name)
    {
        if (TypeNames.Contains(name))
            return;
        
        Add(name);
    }
}