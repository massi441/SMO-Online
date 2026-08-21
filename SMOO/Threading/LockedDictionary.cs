using System.Diagnostics.CodeAnalysis;

namespace SMOO.Threading;

internal class LockedDictionary<TKey, TValue> where TKey : notnull
{
    private readonly Lock _lock = new Lock();
    private readonly Dictionary<TKey, TValue> _dictionary = [];

    public Dictionary<TKey, TValue>.ValueCollection Values => _dictionary.Values;

    public TValue this[TKey key]
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _dictionary[key];
            }
        }
        set
        {
            using (_lock.EnterScope())
            {
                _dictionary[key] = value;
            }
        }
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        using (_lock.EnterScope())
        {
            return _dictionary.TryGetValue(key, out value);
        }
    }

    public bool TryAdd(TKey key, TValue value)
    {
        using (_lock.EnterScope())
        {
            return _dictionary.TryAdd(key, value);
        }
    }

    public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        using (_lock.EnterScope())
        {
            return _dictionary.Remove(key, out value);
        }
    }

    public bool Remove(TKey key)
    {
        return Remove(key, out TValue? _);
    }

    public bool ContainsKey(TKey key)
    {
        using (_lock.EnterScope())
        {
            return _dictionary.ContainsKey(key);
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }

    public void Lock()
    {
        _lock.Enter();
    }

    public void Unlock()
    {
        _lock.Exit();
    }

    public Lock.Scope EnterScope()
    {
        return _lock.EnterScope();
    }
}
