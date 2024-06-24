namespace TurtleLang.Models;

class RuntimeStack
{
    private readonly List<StackFrame> _stack = new();
    public int Count => _stack.Count;

    public void Push(StackFrame stackFrame)
    {
        _stack.Add(stackFrame);
    }

    public StackFrame Pop()
    {
        var item = _stack.Last();

        if (item == null)
            throw new Exception("Runtime stack was empty");
        
        _stack.Remove(item);

        return item;
    }

    public StackFrame Peek()
    {
        var item = _stack.Last();

        if (item == null)
            throw new Exception("Runtime stack was empty");
        
        return item;
    }

    public StackFrame PeekAtIndex(int i)
    {
        // Because it is a list we have to go through it in reverse
        var length = _stack.Count - 1;
        var item = _stack[length - i];
        return item;
    }
}