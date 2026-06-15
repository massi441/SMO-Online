using System.Buffers;

namespace SMOO.Util;

/// <summary>
/// A wrapper around a rented byte buffer from the array pool
/// </summary>
internal class SharedBuffer : IDisposable
{
    private byte[] _ref;
    private int _usedBytes;
    private RefCounter _refCounter;

    public int UsedBytes => _usedBytes;
    public Span<byte> UsedSpan => Ref.AsSpan(0, UsedBytes);
    public byte[] Ref => _ref;
    public int RefCount => _refCounter.Count;

    public SharedBuffer(int size)
    {
        _ref = ArrayPool<byte>.Shared.Rent(size);
        _refCounter.Increment();
        _usedBytes = size;
    }

    /// <summary>
    /// Acquires a reference to the buffer
    /// </summary>
    public void Acquire()
    {
        _refCounter.Increment();
    }

    /// <summary>
    /// Transfers ownership of the buffer. To be called when another thread will require access
    /// to the buffer at a later time than when the buffer is disposed
    /// </summary>
    /// <returns>The shared buffer with an incremented reference count</returns>
    public SharedBuffer Transfer()
    {
        _refCounter.Increment();
        return this;
    }

    /// <summary>
    /// Releases the reference to the current buffer.
    /// The underlying buffer is returned to the array pool if the reference count reaches 0
    /// </summary>
    public bool Release()
    {
        if (_refCounter.Decrement() == 0)
        {
            if (Ref == null)
            {
                return false;
            }

            ArrayPool<byte>.Shared.Return(Ref);

            _ref = null!;
            _usedBytes = 0;

            return true;
        }

        return false;
    }

    public void Restrict(int usedBytes)
    {
        _usedBytes = Math.Min(usedBytes, UsedBytes);
    }

    public void Dispose()
    {
        Release();
    }

    public static implicit operator Span<byte>(SharedBuffer buffer)
    {
        return buffer.UsedSpan;
    }
}
