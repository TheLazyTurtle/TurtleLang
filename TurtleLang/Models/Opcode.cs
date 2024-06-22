namespace TurtleLang.Models;

enum Opcode
{
    If,
    Identifier,
    Value,
    Expression,
    FunctionDefinition,
    Call,
    LoadArgument,
    AddArgument,
    PushStackFrame,
    Return,
    Eof
}