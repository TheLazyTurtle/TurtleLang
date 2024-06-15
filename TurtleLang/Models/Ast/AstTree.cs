namespace TurtleLang.Models.Ast;

class AstTree
{
    public AstNode? Root { get; private set; }

    public void SetRoot(AstNode node)
    {
        if (Root != null)
            throw new Exception("Root is not allowed to be overwritten");

        Root = node;
    }

    public override string ToString()
    {
        return $"{Root}";
    }
}