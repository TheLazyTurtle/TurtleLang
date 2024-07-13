using TurtleLang.Models.Ast;

namespace TurtleLang.Models;

public enum TokenTypes
{
    Var,
    Fn,
    If,
    Else,
    For,
    Eq,
    Gt,
    Lt,
    Gte,
    Lte,
    Add,
    Sub,
    Divide,
    Increase,
    Decrease,
    Comment,
    Struct,
    Impl,
    Self,
    Trait,
    New,
    Assign,
    Identifier,
    LParen,
    RParen,
    LCurly,
    RCurly,
    Semicolon,
    Colon,
    String,
    Int,
    Comma,
    Dot,
    Eof,
}

static class TokenTypesExtensions
{
    public static ExpressionTypes TokenExpressionTypeToExpressionType(this TokenTypes tokenType)
    {
        switch (tokenType)
        {
            case TokenTypes.Eq:
                return ExpressionTypes.Eq;
            case TokenTypes.Gt:
                return ExpressionTypes.Gt;
            case TokenTypes.Lt:
                return ExpressionTypes.Lt;
            case TokenTypes.Gte:
                return ExpressionTypes.Gte;
            case TokenTypes.Lte:
                return ExpressionTypes.Lte;
            case TokenTypes.Assign:
                return ExpressionTypes.Assign;
            case TokenTypes.Fn:
            case TokenTypes.If:
            case TokenTypes.Identifier:
            case TokenTypes.LParen:
            case TokenTypes.RParen:
            case TokenTypes.LCurly:
            case TokenTypes.RCurly:
            case TokenTypes.Semicolon:
            // case TokenTypes.String:
            // case TokenTypes.Int:
            case TokenTypes.Comma:
            case TokenTypes.Eof:
            default:
                throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null);
        }
    }
}
