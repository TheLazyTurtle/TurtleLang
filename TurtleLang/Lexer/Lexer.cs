using System.Diagnostics;
using TurtleLang.Models;

namespace TurtleLang.Lexer;

class Lexer
{
    private string _code = string.Empty;
    private int _currentIndex;
    private string _currentString = string.Empty;
    private int _currentLineNumber = 1; // 1 because a code file starts at line 1 not 0
    private readonly List<Token> _tokens = new();
    
    public List<Token> Lex(string code)
    {
        _code = code;

        while (_currentIndex < _code.Length)
        {
            var currentChar = _code[_currentIndex];
            if (currentChar is ' ' or '\n' or '\r' or '\t')
            {
                if (currentChar is '\n')
                    _currentLineNumber++;
                
                _currentIndex++;
                continue;
            }

            if (currentChar == '\"')
            {
                LexStringValue();
                continue;
            }
            
            _currentString = $"{_currentString}{currentChar}";
            
            var token = CheckToken(_currentString, out var success);

            if (success)
            {
                Debug.Assert(token != null);
                
                AddToken(token);
            }
            
            var nextChar = PeekNextChar();
            _ = CheckToken(nextChar.ToString() ?? "", out success);

            if ((nextChar == ' ' || success) && _currentString.Length > 0)
            {
                AddToken(TokenTypes.Identifier, _currentString);
            }
            
            if (nextChar == null)
                break;
            _currentIndex++;
        }
        
        _tokens.Add(new Token(TokenTypes.Eof, _currentLineNumber));
        return _tokens;
    }
    
    private Token? CheckToken(string token, out bool success)
    {
        success = true;
        switch (token)
        {
            case "fn":
                return new Token(TokenTypes.Fn, _currentLineNumber);
            case "(":
                return new Token(TokenTypes.LParen, _currentLineNumber);
            case ")":
                return new Token(TokenTypes.RParen, _currentLineNumber);
            case "{":
                return new Token(TokenTypes.LCurly, _currentLineNumber);
            case "}":
                return new Token(TokenTypes.RCurly, _currentLineNumber);
            case ";":
                return new Token(TokenTypes.Semicolon, _currentLineNumber);
            case ",":
                return new Token(TokenTypes.Comma, _currentLineNumber);
            default:
                success = false;
                return null;
        }
    }
    
    private void LexStringValue()
    {
        var str = "";
        var c = GetNextChar();
        while (c != '\"')
        {
            str += c;
            c = GetNextChar();
        }

        _currentIndex++; // To skip past the closing "
        
        AddToken(TokenTypes.String, str);
    }

    private char? PeekNextChar()
    {
        if (_currentIndex + 1 >= _code.Length)
            return null; 
        
        return _code[_currentIndex + 1];
    }

    private char? GetNextChar()
    {
        if (_currentIndex + 1 >= _code.Length)
            return null; 
        
        return _code[++_currentIndex];
    }

    private void AddToken(TokenTypes type, string value)
    {
        var token = new Token(type, value, _currentLineNumber);
        AddToken(token);
    }
    
    private void AddToken(TokenTypes type)
    {
        var token = new Token(type, _currentLineNumber);
        AddToken(token);
    }
    
    private void AddToken(Token token)
    {
        _tokens.Add(token);
        _currentString = "";
    }
}