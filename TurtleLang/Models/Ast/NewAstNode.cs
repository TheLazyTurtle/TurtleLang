using System.Diagnostics;
using System.Text;
using TurtleLang.Models.Types;

namespace TurtleLang.Models.Ast;

class NewAstNode: AstNode
{
    private readonly Dictionary<string, ValueAstNode> _valuesByName = new();
    public VariableAstNode VariableAstNode { get; }
    public TypeDefinition Type { get; }
    
    public NewAstNode(Token? token, TypeDefinition typeDefinition, VariableAstNode variableAstNode) : base(Opcode.New, token)
    {
        VariableAstNode = variableAstNode;
        Type = typeDefinition;
    }
    
    public int GetAssignedValueCount()
    {
        return _valuesByName.Count();
    }

    public void AssignField(string fieldName, ValueAstNode value)
    {
        Debug.Assert(!_valuesByName.ContainsKey(fieldName));
        _valuesByName.Add(fieldName, value);
    }

    public Dictionary<string, ValueAstNode> GetAssignedValues()
    {
        return _valuesByName;
    }

    public override string ToString()
    {
        return $"{VariableAstNode} = new {Type}";
    }
    
    public new string ToString(int depth)
    {
        var sb = new StringBuilder();
        var padding = new string(' ', depth * 2); // Double spaces
        var indentDepth = new string(' ', depth * 4); // Quad spaces
        sb.AppendLine($"{padding}{VariableAstNode} = new {Type}");
        
        foreach (var value in _valuesByName)
            sb.AppendLine($"{indentDepth}{value.Key} = {value.Value.ToString()}");

        return sb.ToString();
    }
}