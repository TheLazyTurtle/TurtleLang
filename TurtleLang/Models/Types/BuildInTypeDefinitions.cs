using TurtleLang.Models.Ast;

namespace TurtleLang.Models.Types;

class VoidTypeDefinition : BaseType
{
    public VoidTypeDefinition()
    {
        Name = "void";
        PrimitiveType = PrimitiveType.Void;
    }
}

class IntTypeDefinition: BaseType
{
    public IntTypeDefinition()
    {
        Name = "i32";
        PrimitiveType = PrimitiveType.Int;
    }
}

class StringTypeDefinition: BaseType
{
    public StringTypeDefinition()
    {
        Name = "string";
        PrimitiveType = PrimitiveType.String;
    }
}
