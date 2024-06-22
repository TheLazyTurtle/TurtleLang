namespace TurtleLang.Models;

enum Opcode
{
    If,
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