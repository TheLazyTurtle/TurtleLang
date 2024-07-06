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
                case ':':
                    ConsumeSingleCharToken(TokenTypes.Colon);
                    continue;
                case ',':
                    ConsumeSingleCharToken(TokenTypes.Comma);
                    continue;
                case '=':
                    ConsumeAssignOrEqual();
                    continue;
                case '>':
                    ConsumeGtOrGte();
                    continue;
                case '<':
                    ConsumeLtOrLte();
                    continue;
                case '+':
                    ConsumeAddOrIncrease();
                    continue;
                case '-':
                    ConsumeSubOrDecrease();
                    continue;
                    
            }
            
            GetIdentifierOrKeyword();
        }
        
        _tokens.Add(new Token(TokenTypes.Eof, _currentLineNumber));
        return _tokens;
    }

    private void ConsumeSubOrDecrease()
    {
        var currentChar = _code[_currentIndex];
        Debug.Assert(currentChar == '-');
        
        var nextChar = GetNextChar();
        AddToken(nextChar == '-' ? TokenTypes.Decrease : TokenTypes.Sub);
        GetNextChar();
    }

    private void ConsumeAddOrIncrease()
    {
        var currentChar = _code[_currentIndex];
        Debug.Assert(currentChar == '+');
        
        var nextChar = GetNextChar();
        AddToken(nextChar == '+' ? TokenTypes.Increase : TokenTypes.Add);
        GetNextChar();
    }

    private void ConsumeLtOrLte()
    {
        var currentChar = _code[_currentIndex];
        Debug.Assert(currentChar == '<');
        
        var nextChar = GetNextChar();
        AddToken(nextChar == '=' ? TokenTypes.Lte : TokenTypes.Lt);
        GetNextChar();
    }

    private void ConsumeGtOrGte()
    {
        var currentChar = _code[_currentIndex];
        Debug.Assert(currentChar == '>');
        
        var nextChar = GetNextChar();
        AddToken(nextChar == '=' ? TokenTypes.Gte : TokenTypes.Gt);
        GetNextChar();
    }

    private void ConsumeAssignOrEqual()
    {
        var currentChar = _code[_currentIndex];
        Debug.Assert(currentChar == '=');
        
        var nextChar = GetNextChar();
        AddToken(nextChar == '=' ? TokenTypes.Eq : TokenTypes.Assign);
        GetNextChar();
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
            case "if":
                AddToken(TokenTypes.If);
                return;
            case "else":
                AddToken(TokenTypes.Else);
                return;
            case "for":
                AddToken(TokenTypes.For);
                return;
            case "var":
                AddToken(TokenTypes.Var);
                return;
            case "struct":
                AddToken(TokenTypes.Struct);
                return;
            case "new":
                AddToken(TokenTypes.New);
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

        var intValue = 0;
        
        while (int.TryParse(str, out var result))
        {
            intValue = result;
            var c = GetNextChar();
            str += c;
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
        {
            _currentIndex++; // To make sure we actually exit the loop
            return null; 
        }
        
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