using SMOO.Enumerator;

namespace SMOO.Client;

/// <summary>
/// A wrapper around a list of players with predefined span iterators
/// </summary>
internal readonly struct PlayerList
{
    private readonly Player[] _players;
    public int Length => _players.Length;
    public PlayerActiveEnumerator Active => new PlayerActiveEnumerator(_players);
    public PlayerIgnoreEnumerator Except(Player player) => new PlayerIgnoreEnumerator(_players, player);
    public PlayerSameStageEnumerator SameStageAs(Player player) => new PlayerSameStageEnumerator(_players, player);
    public PlayerInRoomInfoEnumerator PlayerInfos() => new PlayerInRoomInfoEnumerator(_players);
    public PlayerInRoomInfoEnumerator PlayerInfosExcept(Player player) => new PlayerInRoomInfoEnumerator(_players, player);
    public PlayerActiveEnumerator GetEnumerator() => new PlayerActiveEnumerator(_players);

    public PlayerList(int playerCount)
    {
        _players = new Player[playerCount];
    }

    public int ActiveCount() => Active.Count<Player, PlayerActiveEnumerator>();

    public Player this[int index]
    {
        get
        {
            return _players[index];
        }
        set
        {
            _players[index] = value;
        }
    }

    public static implicit operator PlayerActiveEnumerator(PlayerList players)
    {
        return players.GetEnumerator();
    }
}
