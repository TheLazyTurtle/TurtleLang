using System.Diagnostics;
using System.Text;
using TurtleLang.Models.Types;

namespace TurtleLang.Models.Ast;

class FunctionDefinitionAstNode: ScopeableAstNode
{
    public List<VariableAstNode> Locals { get; } = new();
    public List<VariableAstNode>? Arguments { get; set; }
    public int ArgumentCount => Arguments?.Count ?? 0;
    
    public FunctionDefinitionAstNode(Opcode opcode, Token? token) : base(opcode, token)
    {
    }

    public void AddArgument(VariableAstNode variable)
    {
        Arguments ??= new List<VariableAstNode>();
        Arguments.Add(variable);
        
        AddLocal(variable); // All arguments are also locals
    }
    
    public TypeDefinition GetTypeOfArgumentOnIndex(int index)
    {
        return Arguments[index].Type;
    }

    public VariableAstNode? GetArgumentByName(string name)
    {
        return Arguments?.FirstOrDefault(x => x.GetValueAsString() == name);
    }
    
    public VariableAstNode? GetLocalByName(string name)
    {
        return Locals.FirstOrDefault(x => x.GetValueAsString() == name);
    }
    
    public void AddLocal(VariableAstNode variable)
    {
        Debug.Assert(Locals.All(x => x.GetValueAsString() != variable.GetValueAsString()));
        Locals.Add(variable);
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

        if (Children.Count == 0) 
            return sb.ToString();
        
        sb.AppendLine("{");
        foreach (var child in Children)
        {
            if (child is IfAstNode ifNode)
                sb.AppendLine($"{ifNode.ToString(1)}");
            else if (child is ForAstNode forAstNode)
                sb.AppendLine($"{forAstNode.ToString(1)}");
            else if (child is ExpressionAstNode expressionAstNode)
                sb.AppendLine($"{expressionAstNode.ToString(1)}");
            else
                sb.Append(child.ToString(1));
        }

        sb.AppendLine("}");

        return sb.ToString();
    }
}