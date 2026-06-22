namespace SMOO.Threading;

internal struct AtomicBool
{
    private volatile int _value;
    public readonly bool Value => _value == 1;

    public AtomicBool(bool value)
    {
        _value = value ? 1 : 0;
    }

    public void Set(bool value)
    {
        Interlocked.Exchange(ref _value, value ? 1 : 0);
    }

    public static implicit operator bool(AtomicBool atomicBool)
    {
        return atomicBool._value == 1;
    }
}
