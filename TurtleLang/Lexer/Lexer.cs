using System.Diagnostics;
using TurtleLang.Models;
using TurtleLang.Models.Ast;

namespace TurtleLang.Lexer;

class Lexer
{
    private string _code = string.Empty;
    private int _currentIndex;
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
            
            if (char.IsDigit(currentChar))
            {
                LexIntValue();
                continue;
            }

            switch (currentChar)
            {
                case '\"':
                    LexStringValue();
                    continue;
                case '(':
                    ConsumeSingleCharToken(TokenTypes.LParen);
                    continue;
                case ')':
                    ConsumeSingleCharToken(TokenTypes.RParen);
                    continue;
                case '{':
                    ConsumeSingleCharToken(TokenTypes.LCurly);
                    continue;
                case '}':
                    ConsumeSingleCharToken(TokenTypes.RCurly);
                    continue;
                case ';':
                    ConsumeSingleCharToken(TokenTypes.Semicolon);
                    continue;
                case ',':
                    ConsumeSingleCharToken(TokenTypes.Comma);
                    continue;
            }
            
            GetIdentifierOrKeyword();
        }
        
        _tokens.Add(new Token(TokenTypes.Eof, _currentLineNumber));
        return _tokens;
    }

    private void ConsumeSingleCharToken(TokenTypes tokenTypes)
    {
        AddToken(tokenTypes);
        _ = GetNextChar();
    }
    
    private void GetIdentifierOrKeyword()
    {
        var currentChar = _code[_currentIndex];
        var str = "";

        while (char.IsLetterOrDigit(currentChar))
        {
            str += currentChar;
            
            var nextChar = GetNextChar();
            if (nextChar == null)
                break;
            
            currentChar = _code[_currentIndex];
        }

        switch (str)
        {
            case "fn":
                AddToken(TokenTypes.Fn);
                return;
        }
        
        AddToken(new Token(TokenTypes.Identifier, str, _currentLineNumber));
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


    private void LexIntValue()
    {
        var str = "";
        str += _code[_currentIndex];
        
        var c = PeekNextChar();

        var intValue = 0;
        
        while (int.TryParse(str, out var result))
        {
            intValue = result;
            str += c;
            c = GetNextChar();
        }
        
        AddToken(TokenTypes.Int, intValue);
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
    
    private void AddToken(TokenTypes type, int value)
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
    }
}