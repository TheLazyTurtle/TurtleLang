using System.Diagnostics;
using TurtleLang.Models;
using TurtleLang.Models.Ast;
using TurtleLang.Models.Exceptions;
using StackFrame = TurtleLang.Models.StackFrame;

namespace TurtleLang.Lexer;

class Lexer
{
    private readonly AstTree _ast = new();
    private List<Token> _tokens;
    private AstNode _parentNode;
    private Token _currentToken;
    private int _currentIndex;
    private int _curlyCount;
    
    public Dictionary<string, AstNode> FunctionNodesByName { get; } = new();

    public AstTree Lex(List<Token> tokens)
    {
        _parentNode = new AstNode(Opcode.Call, "Main");
        _ast.SetRoot(_parentNode);
        
        _tokens = tokens;

        var token = _tokens[_currentIndex];
        while (_currentIndex < _tokens.Count)
        {
            Check(token);
            token = GetNextToken();

            if (token == null)
                break;
        }
        
        // Validate that there is a main function
        if (!FunctionNodesByName.ContainsKey("Main"))
            throw new Exception("No main function defined");

        return _ast;
    }

    private void Check(Token? token)
    {
        if (token == null)
            return;
        
        AstNode astNode;
        switch (token.TokenType)
        {
            case TokenTypes.Fn:
                if (_curlyCount != 0)
                    throw new InvalidSyntaxException("Curly braces do not close enough before starting new function decl");
                
                Expect(TokenTypes.FunctionIdentifier);
                astNode = new FunctionDefinitionAstNode(Opcode.FunctionDefinition, _currentToken.Value);
                _parentNode.AddSibling(astNode);
                _parentNode = astNode;
                DefineFunction(astNode);
                break;
            case TokenTypes.Call:
                astNode = new AstNode(Opcode.Call, _currentToken.Value);
                Expect(TokenTypes.LParen);
                
                // This indirectly becomes an Expect(RParen) and does not need to be checked again
                while (_currentToken.TokenType != TokenTypes.RParen)
                {
                    var next = GetNextToken();
                    Debug.Assert(next != null);

                    if (next.TokenType == TokenTypes.ArgumentValue)
                    {
                        _parentNode.AddChild(new AstNode(Opcode.PushArgument, next.Value));
                    }
                }
                Expect(TokenTypes.Semicolon);
                _parentNode.AddChild(astNode);
                break;
            case TokenTypes.FunctionIdentifier:
                Expect(TokenTypes.LParen);
                break;
            case TokenTypes.LParen:
                // This indirectly becomes an Expect(RParen) and does not need to be checked again
                while (_currentToken.TokenType != TokenTypes.RParen)
                {
                    var next = GetNextToken();
                    Debug.Assert(next != null);
                    if (next.TokenType == TokenTypes.ArgumentIdentifier)
                    {
                        if (_parentNode is not FunctionDefinitionAstNode functionDefinition)
                            throw new Exception();
                        
                        _parentNode.AddChild(new AstNode(Opcode.LoadArgument, next.Value));
                        functionDefinition.AddArgument();
                    }
                }
                break;
            case TokenTypes.RParen:
                Expect(TokenTypes.LCurly);
                break;
            case TokenTypes.LCurly:
                _curlyCount++;
                // This indirectly becomes an Expect(RCurly) and does not need to be checked again
                while (_currentToken.TokenType != TokenTypes.RCurly)
                    Check(GetNextToken());
                _parentNode.AddSibling(new AstNode(Opcode.Return));
                break;
            case TokenTypes.RCurly:
                _curlyCount--;
                break;
            case TokenTypes.ArgumentIdentifier:
            case TokenTypes.Eof:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private Token? PeekNextToken()
    {
        if (_currentIndex + 1 < _tokens.Count)
        {
            _currentToken = _tokens[_currentIndex + 1];
            return _currentToken;
        }

        return null;
    }
    
    private Token? GetNextToken()
    {
        if (_currentIndex + 1 < _tokens.Count)
        {
            _currentToken = _tokens[++_currentIndex];
            return _currentToken;
        }

        return null;
    }

    private void Expect(TokenTypes expected)
    {
        var next = GetNextToken();
        if (next == null)
            throw new UnexpectedTokenException("Token was null");
        
        if (next.TokenType != expected)
            throw new UnexpectedTokenException($"Expected: {expected.ToString()} got {next}");
    }

    private void DefineFunction(AstNode node)
    {
        if (FunctionNodesByName.ContainsKey(node.Value))
            throw new RedefinitionException(node.Value);
        
        FunctionNodesByName.Add(node.Value, node);
    }
}