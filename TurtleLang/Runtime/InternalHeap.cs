using System.Diagnostics;
using TurtleLang.Models;

namespace TurtleLang.Runtime;

static class InternalHeap
{
    private static int _nextOpenId;
    private static readonly Dictionary<int, RuntimeStruct> Heap = new();

    public static int Malloc(RuntimeStruct item)
    {
        Heap.Add(++_nextOpenId, item);
        return _nextOpenId;
    }

    public static RuntimeStruct GetFromAddress(int addr)
    {
        Debug.Assert(Heap.ContainsKey(addr), "How can you have a mem addr that was never given to you");
        return Heap[addr];
    }

    public static void Free(int addr)
    {
        Heap.Remove(addr);
    }
}