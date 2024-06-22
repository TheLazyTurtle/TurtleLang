﻿using TurtleLang.Models;
using TurtleLang.Models.Ast;
using TurtleLang.Models.BuildIn.Types;
using TurtleLang.Models.Exceptions;
using TurtleLang.Repositories;

namespace TurtleLang.Parser;

class Parser
{
    private readonly AstTree _ast = new();
    private List<Token> _tokens;
    private Stack<AstNode> _parents = new();
    private Token _currentToken;
    private int _currentIndex;
    private int _curlyCount;
    
    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
        
        var callMain = new AstNode(Opcode.Call, new Token(TokenTypes.Identifier, "Main", 0));
        _ast.AddChild(callMain);
        _currentToken = _tokens[0];
    }

    public AstTree Parse()
    {
        while (_currentIndex < _tokens.Count)
        {
            Check(_currentToken);

            if (_currentToken.TokenType == TokenTypes.Eof)
                break;
        }
        
        // Validate that there is a main function
        if (!FunctionDefinitions.Contains("Main"))
            InterpreterErrorLogger.LogError("No main function defined");

        return _ast;
    }

    private void Check(Token token)
    {
        switch (token.TokenType)
        {
            case TokenTypes.Fn:
                if (_curlyCount != 0)
                    InterpreterErrorLogger.LogError("Cannot define function in scope", _currentToken);
                
                Expect(TokenTypes.Identifier);
                var functionDefinition = new FunctionDefinitionAstNode(Opcode.FunctionDefinition, _currentToken);
                DefineFunction(functionDefinition);
                _ast.AddChild(functionDefinition);
                _parents.Push(functionDefinition);
                Expect(TokenTypes.LParen);
                break;
            case TokenTypes.Identifier:
                var expected = PeekNextToken();
                if (expected!.TokenType == TokenTypes.LParen)
                {
                    var callToken = _currentToken;
                    
                    Expect(TokenTypes.LParen);
                    ParseParameterValues(_parents.Peek());
                    _parents.Peek().AddChild(new AstNode(Opcode.PushStackFrame, _currentToken));
                    
                    var callNode = new AstNode(Opcode.Call, callToken);
                    _parents.Peek().AddChild(callNode);
                    
                    Expect(TokenTypes.Semicolon);
                }
                break;
            case TokenTypes.LParen:
                if (_parents.Peek() is FunctionDefinitionAstNode funcDef)
                {
                    ParseParameterDefinition(funcDef);
                }
                break;
            case TokenTypes.LCurly:
                _curlyCount++;
                GetNextToken();
                break;
            case TokenTypes.RCurly:
                _curlyCount--;
                // TODO: Find a better way for implicit returns
                if (_curlyCount == 0)
                {
                    _parents.Peek().AddChild(new AstNode(Opcode.Return, null)); // Implicit return
                    _parents.Pop();
                }
                GetNextToken(); // Just skip it as it has no meaning
                break;
            case TokenTypes.Semicolon:
                GetNextToken(); // Just skip it as it has no meaning
                if (_parents.Peek().Opcode == Opcode.Call)
                    _parents.Pop();
                break;
            case TokenTypes.String:
                GetNextToken(); // Just skip it as it has no meaning
                break;
            case TokenTypes.Comma:
                GetNextToken(); // Just skip it as it has no meaning
                break;
            case TokenTypes.Eof:
            case TokenTypes.RParen:
                GetNextToken(); // Just skip it as it has no meaning
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ParseParameterDefinition(FunctionDefinitionAstNode funcDef)
    {
        GetNextToken();
        while (_currentToken.TokenType != TokenTypes.RParen)
        {
            if (_currentToken.TokenType == TokenTypes.Identifier)
            {
                funcDef.AddArgument(_currentToken.GetValueAsString());
                funcDef.AddChild(new AstNode(Opcode.LoadArgument, _currentToken));
                ExpectEither(TokenTypes.Comma, TokenTypes.RParen);
            }
            else
            {
                GetNextToken();
            }
        }
    }
    
    private void ParseParameterValues(AstNode parentNode)
    {
        GetNextToken();

        while (_currentToken.TokenType != TokenTypes.RParen)
        {
            if (_currentToken.TokenType == TokenTypes.Identifier)
            {
                parentNode.AddChild(new AstNode(Opcode.AddArgument, _currentToken));
                ExpectEither(TokenTypes.Comma, TokenTypes.RParen);
            }
            else if (_currentToken.TokenType == TokenTypes.String)
            {
                parentNode.AddChild(new ArgumentAstNode(Opcode.AddArgument, _currentToken, new BuildInString()));
                ExpectEither(TokenTypes.Comma, TokenTypes.RParen);
            }
            else if (_currentToken.TokenType == TokenTypes.Int)
            {
                parentNode.AddChild(new ArgumentAstNode(Opcode.AddArgument, _currentToken, new BuildInInt()));
                ExpectEither(TokenTypes.Comma, TokenTypes.RParen);
            }
            else
            {
                GetNextToken();
            }
        }
    }

    private Token? PeekNextToken()
    {
        if (_currentIndex + 1 >= _tokens.Count)
            return null;
        
        return _tokens[_currentIndex + 1];
    }
    
    private Token? GetNextToken()
    {
        if (_currentIndex + 1 >= _tokens.Count) 
            return null;

        _currentIndex++;
        _currentToken = _tokens[_currentIndex];
        return _currentToken;
    }

    private void Expect(TokenTypes expected)
    {
        var next = GetNextToken();
        if (next == null)
            throw new UnexpectedTokenException("Token was null");
        
        if (next.TokenType != expected)
            InterpreterErrorLogger.LogError($"Expected: {expected.ToString()} got {next}", _currentToken);
    }

    private void ExpectEither(TokenTypes first, TokenTypes second)
    {
        var next = GetNextToken();
        if (next == null)
            throw new UnexpectedTokenException("Token was null");

        if (next.TokenType != first && next.TokenType != second)
        {
            InterpreterErrorLogger.LogError($"Expected: {first.ToString()} or {second.ToString()} got {next}", _currentToken);
        }
    }

    private void DefineFunction(AstNode node)
    {
        // TODO: Make sure that we cannot redefine a builtin function
        if (FunctionDefinitions.Contains(node.GetValueAsString()))
            InterpreterErrorLogger.LogError($"Redefinition of function with name: {node.GetValueAsString()}", _currentToken);
        
        FunctionDefinitions.Add(node.GetValueAsString(), node);
    }
}