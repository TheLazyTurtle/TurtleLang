using TurtleLang.Models.Ast;
using TurtleLang.Models.Exceptions;

namespace TurtleLang.Runtime;

class RuntimeStack
{
    private readonly Stack<AstNode> _stack = new();

    public void Push(AstNode node)
    {
        _stack.Push(node);
    }

    public AstNode Pop()
    {
        if (_stack.Count == 0)
            throw new StackEmptyException();

        return _stack.Pop();
    }
}