namespace SMOO.Threading;

internal readonly ref struct ScopedReadLock : IDisposable
{
    private readonly ReaderWriterLockSlim _lock;

    public ScopedReadLock(ReaderWriterLockSlim readWriteLock)
    {
        _lock = readWriteLock;
        _lock.EnterReadLock();
    } 

    public void Dispose()
    {
        _lock.ExitWriteLock();
    }
}
