using TurtleLang.Models.Ast;

namespace TurtleLang.Models.Types;

class BaseType: TypeDefinition
{
    public PrimitiveType PrimitiveType { get; init; }
}