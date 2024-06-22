using System.Diagnostics;
using TurtleLang.Models;
using TurtleLang.Models.Ast;
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
                    if (_parents.Peek() is not ScopeableAstNode scopeableAstNode)
                        break;
                        
                    ParseParameterValues(scopeableAstNode);
                    scopeableAstNode.AddChild(new AstNode(Opcode.PushStackFrame, _currentToken));
                    
                    var callNode = new AstNode(Opcode.Call, callToken);
                    scopeableAstNode.AddChild(callNode);
                    
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
                    if (_parents.Peek() is not ScopeableAstNode scopeableParent)
                        break;
                    
                    scopeableParent.AddChild(new AstNode(Opcode.Return, null)); // Implicit return
                }

                if (_parents.Peek() is IfAstNode && PeekNextToken().TokenType == TokenTypes.Else)
                {
                    GetNextToken();
                    break;
                }
                
                _parents.Pop(); 
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
            case TokenTypes.If:
                ParseIfStatement();
                break;
            case TokenTypes.Else:
                ParseElseStatement();
                break;
            case TokenTypes.Eof:
            case TokenTypes.RParen:
                GetNextToken(); // Just skip it as it has no meaning
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ParseElseStatement()
    {
        var elseToken = _currentToken;
        Expect(TokenTypes.LCurly);
        var parentPeek = _parents.Peek();
        
        if (parentPeek is not IfAstNode ifNode)
        {
            InterpreterErrorLogger.LogError("Else needs to be connected to an if");
            return;
        }

        var elseNode = new ElseAstNode(elseToken);
        ifNode.AddElse(elseNode);
        _parents.Pop(); // Remove the if
        _parents.Push(elseNode);
    }

    private void ParseIfStatement()
    {
        var ifToken = _currentToken;
        Expect(TokenTypes.LParen);
        var expression = ParseConditionExpression();
        Expect(TokenTypes.RParen);
        var ifNode = new IfAstNode(expression, ifToken);

        if (_parents.Peek() is not ScopeableAstNode scopeableParent)
            return;
        
        scopeableParent.AddChild(ifNode);
        _parents.Push(ifNode);
    }

    private ExpressionAstNode ParseConditionExpression()
    {
        var parentToken = _currentToken;
        
        ExpectIdentifierOrValue();
        var leftToken = _currentToken;
        var leftAst = ParseIdentifierOrValueTokenToValueAst(leftToken);
        
        ExpectConditionOperator();
        var conditionOperator = _currentToken;
        
        // TODO: Add checks to prevent gt, gte, lt & lte on strings
        
        ExpectIdentifierOrValue();
        var rightToken = _currentToken;
        var rightAst = ParseIdentifierOrValueTokenToValueAst(rightToken);

        return new ExpressionAstNode(leftAst, rightAst, conditionOperator.TokenType.TokenExpressionTypeToExpressionType(), parentToken);
    }

    private AstNode ParseIdentifierOrValueTokenToValueAst(Token token)
    {
        if (token.TokenType == TokenTypes.Identifier)
        {
            return new VariableAstNode(token);
        }
        
        if (token.TokenType == TokenTypes.Int)
        {
            return new ValueAstNode(Opcode.Value, token, BuildInTypes.Int);
        }
        
        Debug.Assert(token.TokenType == TokenTypes.String);
        return new ValueAstNode(Opcode.Value, token, BuildInTypes.String);
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
    
    private void ParseParameterValues(ScopeableAstNode parentNode)
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
                parentNode.AddChild(new ValueAstNode(Opcode.AddArgument, _currentToken, BuildInTypes.String));
                ExpectEither(TokenTypes.Comma, TokenTypes.RParen);
            }
            else if (_currentToken.TokenType == TokenTypes.Int)
            {
                parentNode.AddChild(new ValueAstNode(Opcode.AddArgument, _currentToken, BuildInTypes.Int));
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

    private void ExpectConditionOperator()
    {
        var next = GetNextToken();
        if (next == null)
            throw new Exception("Token was null");
            
        switch (next.TokenType)
        {
            case TokenTypes.Eq:
            case TokenTypes.Gt:
            case TokenTypes.Gte:
            case TokenTypes.Lt:
            case TokenTypes.Lte:
                break;
            default:
                InterpreterErrorLogger.LogError($"Expected condition operator got {next}", _currentToken);
                break;
        }
    }

    private void ExpectIdentifierOrValue()
    {
        var next = GetNextToken();
        
        if (next == null)
            throw new Exception("Token was null");
        
        if (next.TokenType is not (TokenTypes.Identifier or TokenTypes.Int or TokenTypes.String))
            InterpreterErrorLogger.LogError($"Expected identifier or value got {next}", _currentToken);
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