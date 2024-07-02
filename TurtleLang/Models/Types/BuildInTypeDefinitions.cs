using TurtleLang.Models.Ast;

namespace TurtleLang.Models.Types;

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
