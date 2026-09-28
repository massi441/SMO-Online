using SMOO.Client;

namespace SMOO.Enumerator;

/// <summary>
/// An enumerator that maps active players into <see cref="PlayerInRoomInfo"></see>,
/// while excluding a target player from the enumeration (usually the player receiving the player in room infos)
/// </summary>
internal ref struct PlayerInRoomInfoEnumerator : ISpanEnumerator<PlayerInRoomInfo, PlayerInRoomInfoEnumerator>
{
    private PlayerActiveEnumerator _playerEnumerator;
    private PlayerInRoomInfo _current;
    private readonly Player? _exclude;

    public readonly PlayerInRoomInfo Current => _current;
    public readonly PlayerInRoomInfoEnumerator GetEnumerator() => this;

    public PlayerInRoomInfoEnumerator(ReadOnlySpan<Player> players, Player? exclude = null)
    {
        _playerEnumerator = new PlayerActiveEnumerator(players);
        _exclude = exclude;
    }

    public bool MoveNext()
    {
        while (_playerEnumerator.MoveNext())
        {
            if (_playerEnumerator.Current == _exclude)
            {
                continue;
            }

            _current = new PlayerInRoomInfo(_playerEnumerator.Current);
            return true;
        }
        return false;
    }

    public readonly void Dispose() { }
}
