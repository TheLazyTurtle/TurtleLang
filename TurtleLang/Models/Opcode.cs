namespace TurtleLang.Models;

enum Opcode
{
    If,
    Else,
    For,
    Variable,
    Value,
    Expression,
    FunctionDefinition,
    Call,
    LoadArgument,
    AddArgument,
    PushLocalVar,
    PushStackFrame,
    Return,
    Eof
}