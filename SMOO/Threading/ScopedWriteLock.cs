namespace SMOO.Threading;

internal readonly ref struct ScopedWriteLock : IDisposable
{
    private readonly ReaderWriterLockSlim _lock;

    public ScopedWriteLock(ReaderWriterLockSlim readWriteLock)
    {
        _lock = readWriteLock;
        _lock.EnterWriteLock();
    }

    public void Dispose()
    {
        _lock.ExitWriteLock();
    }
}
