using SMOO.Client;

namespace SMOO.Enumerator;

/// <summary>
/// An enumerator of connected players, excluding an ignored player
/// </summary>
internal ref struct PlayerIgnoreEnumerator : IPlayerEnumerator<PlayerIgnoreEnumerator>
{
    private PlayerInStateEnumerator _connectedEnumerator;
    private readonly Player _ignoredPlayer;

    public readonly Player Current => _connectedEnumerator.Current;
    public PlayerIgnoreEnumerator GetEnumerator() => this;

    public PlayerIgnoreEnumerator(ReadOnlySpan<Player> players, Player ignordPlayer)
    {
        _connectedEnumerator = new PlayerInStateEnumerator(players, PlayerState.Connected);
        _ignoredPlayer = ignordPlayer;
    }

    public bool MoveNext()
    {
        bool result = _connectedEnumerator.MoveNext();
        if (_connectedEnumerator.Current == _ignoredPlayer)
        {
            return _connectedEnumerator.MoveNext();
        }

        return result;
    }

    public readonly void Dispose()
    {

    }
}
