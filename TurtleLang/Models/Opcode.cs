namespace TurtleLang.Models;

enum Opcode
{
    FunctionDefinition,
    Call,
    LoadArgument,
    AddArgument,
    PushStackFrame,
    Return,
    Eof
}