namespace SMOO.Threading;

internal readonly ref struct ScopedUpgradeableLock : IDisposable
{
    private readonly ReaderWriterLockSlim _lock;

    public ScopedUpgradeableLock(ReaderWriterLockSlim readWriteLock)
    {
        _lock = readWriteLock;
        _lock.EnterUpgradeableReadLock();
    }

    public void Dispose()
    {
        _lock.ExitUpgradeableReadLock();
    }
}
