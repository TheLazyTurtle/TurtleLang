namespace TurtleLang.Models;

enum Opcode
{
    FunctionDefinition,
    Call,
    LoadArgument,
    PushArgument,
    Return,
    Eof
}