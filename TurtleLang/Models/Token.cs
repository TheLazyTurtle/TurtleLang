namespace TurtleLang.Models;

class Token
{
    public TokenTypes TokenType { get; init; }
    public string Value { get; init; }

    public Token(TokenTypes tokenType, string value)
    {
        TokenType = tokenType;
        Value = value;
    }
    
    public Token(TokenTypes tokenType)
    {
        TokenType = tokenType;
        Value = "";
    }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(Value))
            return TokenType.ToString();

        return $"{TokenType.ToString()} {Value}";
    }
}