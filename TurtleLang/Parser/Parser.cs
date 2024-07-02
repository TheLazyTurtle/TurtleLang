using System.Diagnostics;
using TurtleLang.Models;
using TurtleLang.Models.Ast;
using TurtleLang.Models.Exceptions;
using TurtleLang.Models.Types;
using TurtleLang.Repositories;

namespace TurtleLang.Parser;

class Parser
{
    private readonly AstTree _ast = new();
    private List<Token> _tokens;
    private StackWithIndex<ScopeableAstNode> _parents = new();
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
                ParseFunctionDefinition();
                break;
            case TokenTypes.Var:
                ParseVarDeclaration();
                break;
            case TokenTypes.Identifier:
                ParseIdentifier();
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
                ParseRCurly();
                break;
            case TokenTypes.Semicolon:
                GetNextToken(); // Just skip it as it has no meaning
                if (_parents.Peek().Opcode == Opcode.Call)
                    _parents.Pop();
                break;
            case TokenTypes.String:
            case TokenTypes.Int:
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
            case TokenTypes.For:
                ParseFor();
                break;
            case TokenTypes.Struct:
                ParseStruct();
                break;
            case TokenTypes.Eof:
            case TokenTypes.RParen:
                GetNextToken(); // Just skip it as it has no meaning
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ParseStruct()
    {
        Expect(TokenTypes.Identifier);
        var identifier = _currentToken;
        Expect(TokenTypes.LCurly);

        // var structDefinition = new StructDefinitionAstNode(identifier);
        //
        // while (PeekNextToken()?.TokenType != TokenTypes.RCurly)
        // {
        //     Expect(TokenTypes.Identifier);
        //     var fieldIdentifier = _currentToken;
        //     
        //     Expect(TokenTypes.Identifier);
        //     var typeIdentifier = _currentToken;
        //
        //     var variableAstNode = new VariableAstNode(fieldIdentifier);
        //     var variableDefinition = new VariableDefinition(variableAstNode);
        //     structDefinition.AddField(fieldIdentifier.GetValueAsString(), variableDefinition);
        //     
        //     Expect(TokenTypes.Semicolon);
        // }
        //
        // TypeDefinitions.AddIfNotExists(identifier.GetValueAsString(), structDefinition);
        
        Expect(TokenTypes.RCurly);
    }

    private void ParseVarDeclaration()
    {
        // Var name
        Expect(TokenTypes.Identifier);
        var identifierToken = _currentToken;
        
        Expect(TokenTypes.Colon);
        
        // Type name
        Expect(TokenTypes.Identifier);
        var typeDefinition = _currentToken;
        
        var type = GetType(typeDefinition.GetValueAsString());
        var variableAstNode = new VariableAstNode(identifierToken, type);
        
        // Add variable definition
        var parent = _parents.Peek();
        if (parent is not FunctionDefinitionAstNode funcDef)
            throw new Exception("Add handling for fetching the func def parent");
            
        funcDef.AddLocal(variableAstNode);
        
        // Assign value
        if (PeekNextToken()!.TokenType != TokenTypes.Assign)
        {
            // TODO: Maybe make default value etc
            Expect(TokenTypes.Semicolon);
            return;
        }
        
        Expect(TokenTypes.Assign);
        ExpectEither(TokenTypes.Int, TokenTypes.String);
        var value = _currentToken;
        var valueNode = new ValueAstNode(value, GetBuildInTypeFromToken(value));
        parent.AddChild(new ExpressionAstNode(variableAstNode, valueNode, ExpressionTypes.Assign, _currentToken));
        Expect(TokenTypes.Semicolon);
    }

    // TODO: This function is scuffed, as it does not handle all other cases etc
    private TypeDefinition GetBuildInTypeFromToken(Token token)
    {
        if (token.TokenType == TokenTypes.Int)
            return new IntTypeDefinition();

        if (token.TokenType == TokenTypes.String)
            return new StringTypeDefinition();

        throw new Exception("Type is not known");
    }

    private void ParseFor()
    {
        var forToken = _currentToken;
        Expect(TokenTypes.LParen);
        
        var forAstNode = new ForAstNode(forToken);
        var scopeableAstNode = _parents.Peek();
        
        // Read the variable
        VariableAstNode identifierAst;
        ValueAstNode valueAst;
        Token identifier;
        
        if (scopeableAstNode is not FunctionDefinitionAstNode funcDef)
            throw new Exception("Figure out how to get to funcDef");
        
        if (PeekNextToken()!.TokenType == TokenTypes.Var)
        {
            // Creating new variable
            Expect(TokenTypes.Var);
            Expect(TokenTypes.Identifier);
            identifier = _currentToken;
            
            Expect(TokenTypes.Assign);
            Expect(TokenTypes.Int);
            
            var value = _currentToken;
            var type = GetBuildInTypeFromToken(value);
            valueAst = new ValueAstNode(value, type);
        
            identifierAst = new VariableAstNode(identifier, valueAst.Type);
            funcDef.AddLocal(identifierAst);
        }
        else
        {
            // Using existing variable
            Expect(TokenTypes.Identifier);
            identifier = _currentToken;
            Expect(TokenTypes.Assign);
        
            ExpectIdentifierOrValue();
            var value = _currentToken;
            var type = GetBuildInTypeFromToken(value);
            valueAst = new ValueAstNode(value, type);
        
            var existingVariableDefinition = funcDef.GetLocalByName(identifier.GetValueAsString());
            if(existingVariableDefinition == null) 
            {
                InterpreterErrorLogger.LogError("Variable has not yet been assigned in scope", identifier);
                return;
            }
                
            identifierAst = existingVariableDefinition;
        }
        
        Expect(TokenTypes.Semicolon);
        
        forAstNode.AddInitializer(new ExpressionAstNode(identifierAst, valueAst, ExpressionTypes.Assign, identifier));
        
        // Read the expr
        var expression = ParseConditionExpression();
        forAstNode.AddExpression(expression);
        Expect(TokenTypes.Semicolon);
        
        // The operation increment or decrement etc
        Expect(TokenTypes.Identifier);
        var operationIdentifier = _currentToken;
        var variableDefinition = funcDef.GetLocalByName(operationIdentifier.GetValueAsString());
        var operationIdentifierAstNode = new VariableAstNode(operationIdentifier, variableDefinition.Type);
        
        ExpectEither(TokenTypes.Increase, TokenTypes.Decrease);
        var operation = _currentToken;
        var expressionType = operation.TokenType == TokenTypes.Increase ? ExpressionTypes.Increase : ExpressionTypes.Decrease;
        forAstNode.AddIncrementExpression(new ExpressionAstNode(operationIdentifierAstNode, expressionType, operationIdentifier));
        
        scopeableAstNode.AddChild(forAstNode);
        _parents.Push(forAstNode);
        
        GetNextToken();
    }
    
    private void ParseFunctionDefinition()
    {
        if (_curlyCount != 0)
            InterpreterErrorLogger.LogError("Cannot define function in scope", _currentToken);
        
        Expect(TokenTypes.Identifier);
        var functionDefinition = new FunctionDefinitionAstNode(Opcode.FunctionDefinition, _currentToken);
        
        DefineFunction(functionDefinition);
        _ast.AddChild(functionDefinition);
        _parents.Push(functionDefinition);
        Expect(TokenTypes.LParen);
    }

    private void ParseIdentifier()
    {
        var expected = PeekNextToken();
        if (expected!.TokenType != TokenTypes.LParen)
        {
            ParseVariableOperation();
            return;
        }
        
        var callToken = _currentToken;
            
        Expect(TokenTypes.LParen);
        if (_parents.Peek() is not ScopeableAstNode scopeableAstNode)
            return;
                
        var callNode = new AstNode(Opcode.Call, callToken);
        ParseParameterValues(callNode);
            
        FunctionDefinitions.Add(callToken.GetValueAsString(), null);
        scopeableAstNode.AddChild(callNode);
            
        Expect(TokenTypes.Semicolon);
    }

    private void ParseVariableOperation()
    {
        var variableToken = _currentToken;
        
        ExpectVariableModificationOperator();
        
        var operatorToken = _currentToken;
        
        ExpectIdentifierOrValue();
        var value = _currentToken;
        var valueType = GetBuildInTypeFromToken(value);
        var valueAstNode = new ValueAstNode(value, valueType);
        
        var parent = _parents.Peek();
        if (parent is not FunctionDefinitionAstNode funcDef)
            throw new Exception("Find a way to get to the funcDef");
        
        var variableDefinition = funcDef.GetLocalByName(variableToken.GetValueAsString());
        if (variableDefinition == null)
        {
            InterpreterErrorLogger.LogError("Undeclared variable cannot be modified.", variableToken);
            return;
        }
        
        var expression = new ExpressionAstNode(variableDefinition, valueAstNode, operatorToken.TokenType.TokenExpressionTypeToExpressionType(), variableToken);
        parent.AddChild(expression);
    }

    private void ParseRCurly()
    {
        _curlyCount--;
        // TODO: Find a better way for implicit returns
        if (_curlyCount == 0)
        {
            if (_parents.Peek() is not ScopeableAstNode scopeableParent)
                return;
            
            scopeableParent.AddChild(new AstNode(Opcode.Return, null)); // Implicit return
        }

        if (_parents.Peek() is IfAstNode && PeekNextToken().TokenType == TokenTypes.Else)
        {
            GetNextToken();
            return;
        }
        
        _parents.Pop(); 
        GetNextToken(); // Just skip it as it has no meaning
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

    private ValueAstNode ParseIdentifierOrValueTokenToValueAst(Token token)
    {
        if (token.TokenType == TokenTypes.Identifier)
        {
            var parent = _parents.Peek();
            if (parent is not FunctionDefinitionAstNode funcDef)
                throw new Exception("Find a way to get funcDef");
            
            var variableNode = funcDef.GetLocalByName(_currentToken.GetValueAsString());
            if (variableNode == null)
            {
                InterpreterErrorLogger.LogError("Variable was not defined in scope.", _currentToken);
            }
            
            return new VariableAstNode(token, variableNode.Type);
        }
        
        if (token.TokenType == TokenTypes.Int)
        {
            return new ValueAstNode(token, new IntTypeDefinition());
        }
        
        Debug.Assert(token.TokenType == TokenTypes.String);
        return new ValueAstNode(token, new StringTypeDefinition());
    }

    private void ParseParameterDefinition(FunctionDefinitionAstNode funcDef)
    {
        GetNextToken();
        while (_currentToken.TokenType != TokenTypes.RParen)
        {
            if (_currentToken.TokenType == TokenTypes.Identifier)
            {
                var identifierNameToken = _currentToken;
                Expect(TokenTypes.Colon);
                Expect(TokenTypes.Identifier);
                var typeName = _currentToken;
                TypeDefinitions.AddOrDefine(typeName.GetValueAsString(), null);
                var type = GetType(typeName.GetValueAsString());
        
                var variableAstNode = new VariableAstNode(identifierNameToken, type);
        
                funcDef.AddArgument(variableAstNode);
                ExpectEither(TokenTypes.Comma, TokenTypes.RParen);
            }
            else
            {
                GetNextToken();
            }
        }
    }

    private TypeDefinition GetType(string typeName)
    {
        return TypeDefinitions.GetByName(typeName);
    }

    private void ParseParameterValues(AstNode callNode)
    {
        GetNextToken();
        
        while (_currentToken.TokenType != TokenTypes.RParen)
        {
            if (_currentToken.TokenType == TokenTypes.Identifier)
            {
                var funcDef = GetFirstFunctionDefinitionNode();
                
                var variableNode = funcDef.GetLocalByName(_currentToken.GetValueAsString());
                if (variableNode == null)
                {
                    InterpreterErrorLogger.LogError("Variable was not defined in scope.", _currentToken);
                    return;
                }
                callNode.AddChild(new VariableAstNode(_currentToken, variableNode.Type));
                ExpectEither(TokenTypes.Comma, TokenTypes.RParen);
            }
            else if (_currentToken.TokenType == TokenTypes.String)
            {
                callNode.AddChild(new ValueAstNode(_currentToken, new StringTypeDefinition()));
                ExpectEither(TokenTypes.Comma, TokenTypes.RParen);
            }
            else if (_currentToken.TokenType == TokenTypes.Int)
            {
                callNode.AddChild(new ValueAstNode(_currentToken, new IntTypeDefinition()));
                ExpectEither(TokenTypes.Comma, TokenTypes.RParen);
            }
            else
            {
                GetNextToken();
            }
        }
    }

    private FunctionDefinitionAstNode GetFirstFunctionDefinitionNode()
    {
        var index = 0;
        var parent = _parents.PeekAtIndex(index);
        
        while (parent is not FunctionDefinitionAstNode)
        {
            index++;
            parent = _parents.PeekAtIndex(index);
        }
        
        Debug.Assert(_parents.PeekAtIndex(index) is FunctionDefinitionAstNode);
        return _parents.PeekAtIndex(index) as FunctionDefinitionAstNode;
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

    private void ExpectVariableModificationOperator()
    {
        var next = GetNextToken();
        if (next == null)
            throw new Exception("Token was null");

        switch (next.TokenType)
        {
            case TokenTypes.Assign:
            case TokenTypes.Add:
            case TokenTypes.Decrease:
                break;
            default:
                InterpreterErrorLogger.LogError($"Expected variable modification operator got {next}", _currentToken);
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