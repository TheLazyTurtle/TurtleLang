using TurtleLang.Models.Ast;

namespace TurtleLang.Models;

public enum TokenTypes
{
    Fn,
    If,
    Else,
    Eq,
    Gt,
    Lt,
    Gte,
    Lte,
    Assign,
    Identifier,
    LParen,
    RParen,
    LCurly,
    RCurly,
    Semicolon,
    String,
    Int,
    Comma,
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
            case TokenTypes.Fn:
            case TokenTypes.If:
            case TokenTypes.Assign:
            case TokenTypes.Identifier:
            case TokenTypes.LParen:
            case TokenTypes.RParen:
            case TokenTypes.LCurly:
            case TokenTypes.RCurly:
            case TokenTypes.Semicolon:
            case TokenTypes.String:
            case TokenTypes.Int:
            case TokenTypes.Comma:
            case TokenTypes.Eof:
            default:
                throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null);
        }
    }
}
