namespace TurtleLang.Models.Ast;

class BuildInFunctionAstNode: AstNode
{
    public List<string>? Arguments { get; set; }
    public int ArgumentCount => Arguments?.Count ?? 0;
    
    public string Name { get; private set; }
    public Action<AstNode> Handler { get; private set; }
    
    public BuildInFunctionAstNode(string name, Action<AstNode> handler, List<string>? argumentNames) : base(Opcode.Call, null)
    {
        Name = name;
        Handler = handler;
        Arguments = argumentNames;
    }
}