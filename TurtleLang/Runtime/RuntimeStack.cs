using TurtleLang.Models;
using TurtleLang.Models.Ast;
using TurtleLang.Models.Exceptions;

namespace TurtleLang.Runtime;

class RuntimeStack
{
    private readonly Stack<StackFrame> _stack = new();

    public void Push(StackFrame frame)
    {
        _stack.Push(frame);
    }

    public StackFrame Pop()
    {
        if (_stack.Count == 0)
            throw new StackEmptyException();

        return _stack.Pop();
    }

    public StackFrame Peek()
    {
        if (_stack.Count == 0)
            throw new StackEmptyException();

        return _stack.Peek();
    }
}