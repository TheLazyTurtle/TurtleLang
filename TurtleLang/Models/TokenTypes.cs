namespace TurtleLang.Models;

public enum TokenTypes
{
    Global,
    Fn,
    FunctionIdentifier,
    ArgumentIdentifier,
    ArgumentValue,
    LParen,
    RParen,
    LCurly,
    RCurly,
    Call,
    Semicolon,
    Eof,
}