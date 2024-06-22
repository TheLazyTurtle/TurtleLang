namespace TurtleLang.Models;

class Token
{
    private object _value;
    public TokenTypes TokenType { get; }
    public int LineNumber { get; }

    public Token(TokenTypes tokenType, string value, int lineNumber)
    {
        TokenType = tokenType;
        _value = value;
        LineNumber = lineNumber;
    }
    
    public Token(TokenTypes tokenType, int value, int lineNumber)
    {
        TokenType = tokenType;
        _value = value;
        LineNumber = lineNumber;
    }
    
    public Token(TokenTypes tokenType, int lineNumber)
    {
        TokenType = tokenType;
        _value = "";
        LineNumber = lineNumber;
    }

    public string GetValueAsString()
    {
        return (string) _value;
    }

    public int GetValueAsInt()
    {
        return (int)_value;
    }

    public override string ToString()
    {
        if (TokenType == TokenTypes.Int)
            return $"{TokenType.ToString()} {GetValueAsInt()}";
            
        if (string.IsNullOrEmpty(GetValueAsString()))
            return TokenType.ToString();

        return $"{TokenType.ToString()} {GetValueAsString()}";
    }
}