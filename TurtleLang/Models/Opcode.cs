namespace TurtleLang.Models;

enum Opcode
{
    If,
    Else,
    Variable,
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