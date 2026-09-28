using System.Net;
using SMOO.Server;
using SMOO.Services.Impl;

namespace SMOO.Client;

internal class PlayerHolder : IPlayerHolder
{
    private readonly ServerContext _context;
    private readonly PlayerList _players;

    internal const byte DefaultRoomSize = 4;
    internal const byte MaxRoomSize = 10;
    internal static readonly string DefaultCostumeName = "Mario";

    public PlayerList Players => _players;
    public byte MaxSize => (byte)_players.Length;

    public PlayerHolder(ServerContext context, byte size = DefaultRoomSize)
    {
        _context = context;
        _players = new PlayerList(Math.Min(size, MaxRoomSize));
    }

    public ServerResult<Player> RegisterPlayer(PlayerRegisterInfo playerInfo)
    {

        if (ContainsPlayer(playerInfo))
        {
            return ServerResult<Player>.Failure(ServerError.PlayerAlreadyInRoom);
        }

        if (!TryFindFreeSlot(out byte index))
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
                CostumeBody = DefaultCostumeName,
                CostumeCap = DefaultCostumeName
            },
            SyncData = new PlayerSyncData(),
            SequencedPacketStore = new SequencedPacketStore(_context)
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
                player.SequencedPacketStore.Clear();

                _players[i] = null!;

                return ServerResult.Success();
            }
        }

        return ServerResult.Failure(ServerError.PlayerNotFound);
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

    private bool TryFindFreeSlot(out byte index)
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

    private bool ContainsPlayer(PlayerRegisterInfo playerInfo)
    {
        return FindPlayerByHost(playerInfo.Endpoint) != null;
    }
}
