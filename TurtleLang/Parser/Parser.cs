using System.Diagnostics;
using System.Net;
using System.Text;
using TurtleLang.Models;

namespace TurtleLang.Parser;

class Parser
{
    private string _code;
    private int _currentIndex;
    private string _currentString;
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
                _tokens.Add(new Token(TokenTypes.Eof));
                break;
            }
            
            if (nextChar == '(')
            {
                if (_prevToken.TokenType == TokenTypes.Fn)
                {
                    // Defining a function
                    var identifierToken = new Token(TokenTypes.FunctionIdentifier, _currentString);
                    _tokens.Add(identifierToken);
                    _prevToken = identifierToken;
                    _currentString = "";
                }
                else
                {
                    // Calling a function
                    var callingToken = new Token(TokenTypes.Call, _currentString);
                    _tokens.Add(callingToken);
                    _prevToken = callingToken;
                    _currentString = "";
                }
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
                return new Token(TokenTypes.Fn);
            case "(":
                return new Token(TokenTypes.LParen);
            case ")":
                return new Token(TokenTypes.RParen);
            case "{":
                return new Token(TokenTypes.LCurly);
            case "}":
                return new Token(TokenTypes.RCurly);
            case ";":
                return new Token(TokenTypes.Semicolon);
            default:
                success = false;
                return null;
        }
    }

    private void ParseArgs()
    {
        if (_prevToken.TokenType == TokenTypes.Call)
        {
            // This is for when we try to pass an argument
            ParseFunctionCallingArgs();
            return;
        }

        ParseFunctionDefinitionArgs();
    }

    private void ParseFunctionCallingArgs()
    {
        var sb = new StringBuilder();
        
        while (PeekNextChar() != ')')
        {
            // TODO: Handle spaces in variable
            // TODO: Handle trailing comma
            var nextChar = GetNextChar();
            
            // End of var
            if (nextChar == ',')
            {
                _tokens.Add(new Token(TokenTypes.ArgumentValue, sb.ToString().Trim()));
                sb.Clear();
                continue;
            }
            sb.Append(nextChar);
        }

        if (sb.Length == 0) 
            return;
        
        var token = new Token(TokenTypes.ArgumentValue, sb.ToString().Trim());
        _tokens.Add(token);
    }

    private void ParseFunctionDefinitionArgs()
    {
        var sb = new StringBuilder();
        
        while (PeekNextChar() != ')')
        {
            // TODO: Handle spaces in variable
            // TODO: Handle trailing comma
            var nextChar = GetNextChar();
            
            // End of var
            if (nextChar == ',')
            {
                _tokens.Add(new Token(TokenTypes.ArgumentIdentifier, sb.ToString().Trim()));
                sb.Clear();
                continue;
            }
            sb.Append(nextChar);
        }

        if (sb.Length == 0) 
            return;
        
        var token = new Token(TokenTypes.ArgumentIdentifier, sb.ToString().Trim());
        _tokens.Add(token);
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
}