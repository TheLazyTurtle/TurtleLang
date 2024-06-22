using System.Text;
using TurtleLang.Models.Scopes;

namespace TurtleLang.Models.Ast;

class FunctionDefinitionAstNode: ScopeableAstNode
{
    public List<string>? Arguments { get; set; }
    public int ArgumentCount => Arguments?.Count ?? 0;
    
    public FunctionDefinitionAstNode(Opcode opcode, Token? token) : base(opcode, token)
    {
    }
    
    public void AddScope(FunctionScope scope)
    {
        if (Scope != null)
            InterpreterErrorLogger.LogError("Function already had an scope");

        Scope = scope;
    }

    public void AddArgument(string identifier)
    {
        Arguments ??= new List<string>();
        Arguments.Add(identifier);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=========================");
        sb.Append(GetValueAsString());

        if (Arguments != null)
            sb.AppendLine($"({string.Join(',', Arguments)})");
        else
            sb.AppendLine("()");

        if (Scope == null) 
            return sb.ToString();
        
        sb.AppendLine("{");
        foreach (var child in Scope.Children)
        {
            if (child is IfAstNode ifNode)
                sb.AppendLine($"{ifNode.ToString(1)}");
            else
                sb.Append(child.ToString(1));
        }

        sb.AppendLine("}");


        return sb.ToString();
    }
}