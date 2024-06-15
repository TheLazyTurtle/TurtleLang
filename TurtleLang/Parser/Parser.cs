using System.Diagnostics;
using System.Text;
using TurtleLang.Models;

namespace TurtleLang.Parser;

class Parser
{
    private string _code;
    private int _currentIndex;
    private string _currentString;
    private int _currentLineNumber = 1; // 1 because a code file starts at line 1 not 0
    private Token _prevToken;
    private readonly List<Token> _tokens = new();
    
    public List<Token> Parse(string filePath)
    {
        _code = File.ReadAllText(filePath);

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
            
            _currentString = $"{_currentString}{currentChar}";
            
            var token = CheckToken(_currentString, out var success);

            if (success)
            {
                Debug.Assert(token != null);
                
                _tokens.Add(token);
                
                if (token.TokenType == TokenTypes.LParen)
                    ParseArgs();
                
                _prevToken = token;
                _currentString = "";
            }

            var nextChar = PeekNextChar();
            
            if (nextChar == null)
            {
                _tokens.Add(new Token(TokenTypes.Eof, _currentLineNumber));
                break;
            }
            
            if (nextChar == '(')
            {
                Token functionToken;
                // Defining a function or calling a function
                if (_prevToken.TokenType == TokenTypes.Fn)
                    functionToken = new Token(TokenTypes.FunctionIdentifier, _currentString, _currentLineNumber);
                else
                    functionToken = new Token(TokenTypes.Call, _currentString, _currentLineNumber);
                _tokens.Add(functionToken);
                _prevToken = functionToken;
                _currentString = "";
            }
            _currentIndex++;
        }
        

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
            default:
                success = false;
                return null;
        }
    }

    private void ParseArgs()
    {
        var isStaticString = false;
        var tokenType = _prevToken.TokenType == TokenTypes.Call ? TokenTypes.ArgumentValue : TokenTypes.ArgumentIdentifier;
        
        var sb = new StringBuilder();
        
        while (PeekNextChar() != ')')
        {
            var nextChar = GetNextChar();
            if (nextChar == '\"')
                isStaticString = !isStaticString;

            if (nextChar == ',' &&  PeekNextCharSkipAllWhiteSpaces() == ')')
                InterpreterErrorLogger.LogError("Trailing comma is not allowed", _prevToken);

            if (!isStaticString && nextChar == ' ' && PeekNextChar() != ',')
                InterpreterErrorLogger.LogError("Variables are not allowed to have spaces", _prevToken);
            
            // End of var
            if (nextChar == ',')
            {
                _tokens.Add(new Token(tokenType, sb.ToString().Trim(), _currentLineNumber));
                sb.Clear();

                // Skip whitespace after comma if it is there
                if (PeekNextChar() == ' ')
                    GetNextChar();
                
                continue;
            }
            sb.Append(nextChar);
        }

        if (sb.Length == 0) 
            return;
        
        var token = new Token(tokenType, sb.ToString().Trim(), _currentLineNumber);
        _tokens.Add(token);
    }

    private char? PeekNextChar()
    {
        if (_currentIndex + 1 >= _code.Length)
            return null; 
        
        return _code[_currentIndex + 1];
    }
    
    private char? PeekNextCharSkipAllWhiteSpaces()
    {
        if (_currentIndex + 1 >= _code.Length)
            return null;

        var index = 1;
        var curChar = _code[_currentIndex + index];
        while (curChar == ' ')
        {
            curChar = _code[_currentIndex + ++index];
        }
        return _code[_currentIndex + index];
    }

    private char? GetNextChar()
    {
        if (_currentIndex + 1 >= _code.Length)
            return null; 
        
        return _code[++_currentIndex];
    }
}