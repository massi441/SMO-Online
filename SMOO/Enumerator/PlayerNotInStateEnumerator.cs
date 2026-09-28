using SMOO.Client;

namespace SMOO.Enumerator;

/// <summary>
/// An enumerator of players that are not in a given state
/// </summary>
internal ref struct PlayerNotInStateEnumerator : IPlayerEnumerator<PlayerNotInStateEnumerator>
{
    private PlayerActiveEnumerator _activeEnumerator;
    private readonly PlayerState _excludedState;

    public readonly Player Current => _activeEnumerator.Current;
    public PlayerNotInStateEnumerator GetEnumerator() => this;

    public PlayerNotInStateEnumerator(ReadOnlySpan<Player> players, PlayerState excludedState)
    {
        _activeEnumerator = new PlayerActiveEnumerator(players);
        _excludedState = excludedState;
    }

    public bool MoveNext()
    {
        while (_activeEnumerator.MoveNext())
        {
            if (_activeEnumerator.Current.State != _excludedState)
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
