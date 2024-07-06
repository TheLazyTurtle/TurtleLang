namespace TurtleLang.Models.Types;

class TypeDefinition: IEquatable<TypeDefinition>
{
    protected string Name { get; init; }

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