namespace TurtleLang.Models;

class Token
{
    public TokenTypes TokenType { get; }
    public string Value { get; }
    public int LineNumber { get; }

    public Token(TokenTypes tokenType, string value, int lineNumber)
    {
        TokenType = tokenType;
        Value = value;
        LineNumber = lineNumber;
    }
    
    public Token(TokenTypes tokenType, int lineNumber)
    {
        TokenType = tokenType;
        Value = "";
        LineNumber = lineNumber;
    }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(Value))
            return TokenType.ToString();

        return $"{TokenType.ToString()} {Value}";
    }
}