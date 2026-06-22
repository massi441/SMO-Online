namespace SMOO.Threading;

internal static class ReadWriteLockExtensions
{
    public static ScopedReadLock EnterReadScope(this ReaderWriterLockSlim readWriteLock)
    {
        return new ScopedReadLock(readWriteLock);
    }

    public static ScopedUpgradeableLock EnterUpgradeableScope(this ReaderWriterLockSlim readWriteLock)
    {
        return new ScopedUpgradeableLock(readWriteLock);
    } 

    public static ScopedWriteLock EnterWriteScope(this ReaderWriterLockSlim writeWriteLock)
    {
        return new ScopedWriteLock(writeWriteLock);
    }
}
