using SMOO.Client;

namespace SMOO.Enumerator;

/// <summary>
/// An enumerator of players that are not disconnecting, excluding an ignored player
/// </summary>
internal ref struct PlayerIgnoreEnumerator : IPlayerEnumerator<PlayerIgnoreEnumerator>
{
    private PlayerNotInStateEnumerator _playerEnumerator;
    private readonly Player _ignoredPlayer;

    public readonly Player Current => _playerEnumerator.Current;
    public PlayerIgnoreEnumerator GetEnumerator() => this;

    public PlayerIgnoreEnumerator(ReadOnlySpan<Player> players, Player ignordPlayer)
    {
        _playerEnumerator = new PlayerNotInStateEnumerator(players, PlayerState.Disconnecting);
        _ignoredPlayer = ignordPlayer;
    }

    public bool MoveNext()
    {
        bool result = _playerEnumerator.MoveNext();
        if (_playerEnumerator.Current == _ignoredPlayer)
        {
            return _playerEnumerator.MoveNext();
        }

        return result;
    }

    public readonly void Dispose()
    {

    }
}
