namespace SMOO.Threading;

internal struct AtomicCounter
{
    private int _count;

    public readonly int Count => _count;

    public int Increment()
    {
        return Interlocked.Increment(ref _count);
    }

    public int Decrement()
    {
        return Interlocked.Decrement(ref _count);
    }
}
