using SMOO.Client;

namespace SMOO.Enumerator;

/// <summary>
/// An enumerator of players in a given state
/// </summary>
internal ref struct PlayerInStateEnumerator : IPlayerEnumerator<PlayerInStateEnumerator>
{
    private PlayerActiveEnumerator _activeEnumerator;
    private readonly PlayerState _targetState;

    public readonly Player Current => _activeEnumerator.Current;
    public PlayerInStateEnumerator GetEnumerator() => this;

    public PlayerInStateEnumerator(ReadOnlySpan<Player> players, PlayerState targetState)
    {
        _activeEnumerator = new PlayerActiveEnumerator(players);
        _targetState = targetState;
    }

    public bool MoveNext()
    {
        while (_activeEnumerator.MoveNext())
        {
            if (_activeEnumerator.Current.State == _targetState)
            {
                return true;
            }
        }

        return false;
    }

    public readonly void Dispose()
    {

    }
}
