namespace TurtleLang.Models;

class StackWithIndex<T>
{
    private readonly List<T> _stack = new();
    public int Count => _stack.Count;

    public void Push(T stackFrame)
    {
        _stack.Add(stackFrame);
    }

    public T Pop()
    {
        var item = _stack.Last();

        if (item == null)
            throw new Exception("Stack with index was empty");
        
        _stack.Remove(item);

        return item;
    }

    public T Peek()
    {
        var item = _stack.Last();

        if (item == null)
            throw new Exception("Stack with index was empty");
        
        return item;
    }

    public T PeekAtIndex(int i)
    {
        // Because it is a list we have to go through it in reverse
        var length = _stack.Count - 1;
        var item = _stack[length - i];
        return item;
    }
}