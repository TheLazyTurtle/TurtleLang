namespace TurtleLang.Models.Ast;

class BuildInFunctionAstNode: AstNode
{
    public List<VariableDefinition>? Arguments { get; set; }
    public int ArgumentCount => Arguments?.Count ?? 0;

    public bool InfiniteAmountOfParameters { get; private set; }
    
    public string Name { get; private set; }
    public Action<AstNode> Handler { get; private set; }
    
    public BuildInFunctionAstNode(string name, Action<AstNode> handler, List<VariableDefinition>? argumentNames) : base(Opcode.Call, null)
    {
        Name = name;
        Handler = handler;
        Arguments = argumentNames;
    }
    
    public BuildInFunctionAstNode(string name, Action<AstNode> handler, bool infiniteAmountOfParameters) : base(Opcode.Call, null)
    {
        Name = name;
        Handler = handler;
        Arguments = null;
        InfiniteAmountOfParameters = infiniteAmountOfParameters;
    }
    
    public BuildInTypes GetTypeOfArgumentOnIndex(int index)
    {
        return Arguments[index].Type;
    }
}