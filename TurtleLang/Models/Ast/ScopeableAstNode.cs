using System.Diagnostics;

namespace TurtleLang.Models.Ast;

class ScopeableAstNode : AstNode
{
    public List<VariableDefinition> Locals { get; } = new();
    
    public ScopeableAstNode(Opcode opcode, Token? token) : base(opcode, token)
    {
    }
    
    public VariableDefinition? GetLocalByName(string name)
    {
        return Locals.FirstOrDefault(x => x.Name == name);
    }
    
    public void AddLocal(VariableDefinition variable)
    {
        Debug.Assert(Locals.All(x => x.Name != variable.Name));
        Locals.Add(variable);
    }
}