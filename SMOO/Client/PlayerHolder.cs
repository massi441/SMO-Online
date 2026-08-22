using System.Net;
using SMOO.Server;

namespace SMOO.Client;

internal class PlayerHolder : IPlayerHolder
{
    private readonly PlayerList _players;
    public PlayerList Players => _players;
    public byte MaxSize => (byte)_players.Length;

    public PlayerHolder(byte size = Constants.DefaultRoomSize)
    {
        _players = new PlayerList(Math.Min(size, Constants.MaxRoomSize));
    }

    public ServerResult<Player> RegisterPlayer(in PlayerInfo playerInfo)
    {
        if (ContainsPlayer(playerInfo))
        {
            return ServerResult<Player>.Failure(ServerError.PlayerAlreadyInRoom);
        }

        if (!TryFindSlot(out byte index))
        {
            return ServerResult<Player>.Failure(ServerError.RoomFull);
        }

        Player player = new Player()
        {
            Id = new PlayerId()
            {
                Endpoint = playerInfo.Endpoint,
                SessionId = Guid.NewGuid(),
            },
            Slot = index,
            Name = playerInfo.Name,
            Room = playerInfo.Room,
            WorldInfo = new PlayerWorldInfo()
            {
                CurrentStage = string.Empty,
                CostumeBody = Constants.DefaultCostumeName,
                CostumeCap = Constants.DefaultCostumeName
            },
            SyncData = new PlayerSyncData()
        };

        _players[index] = player;

        return ServerResult<Player>.Success(player);
    }

    public ServerResult UnregisterPlayer(Player player)
    {
        for (int i = 0; i < _players.Length; i++)
        {
            if (_players[i] == player)
            {
                _players[i] = null!;

                return ServerResult.Success();
            }
        }

        return ServerResult.Failure(ServerError.OperationFailed);
    }

    public Player? FindPlayerById(PlayerId id)
    {
        foreach (Player p in _players)
        {
            if (p == null)
            {
                continue;
            }

            if (p.Id == id)
            {
                return p;
            }
        }

        return null;
    }

    public Player? FindPlayerByHost(IPEndPoint endpoint)
    {
        foreach (Player p in _players)
        {
            if (p == null)
            {
                continue;
            }

            if (p.Endpoint.Equals(endpoint))
            {
                return p;
            }
        }

        return null;
    }

    // TODO: Merge into one single operation

    private bool TryFindSlot(out byte index)
    {
        index = 0;

        while (index < _players.Length)
        {
            if (_players[index] == null)
            {
                return true;
            }

            index++;
        }

        return false;
    }

    private bool ContainsPlayer(PlayerInfo playerInfo)
    {
        return FindPlayerByHost(playerInfo.Endpoint) != null;
    }
}
