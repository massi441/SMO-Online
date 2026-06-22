using System.Net;
using SMOO.Server;
using SMOO.Threading;

namespace SMOO.Client;

internal class PlayerHolder : IPlayerHolder
{
    private readonly PlayerList _players;
    public PlayerList Players => _players;
    public byte MaxSize => (byte)_players.Length;
    public ReaderWriterLockSlim ReadWriteLock { get; }

    public PlayerHolder(byte size = Constants.DefaultRoomSize)
    {
        _players = new PlayerList(Math.Min(size, Constants.MaxRoomSize));
        ReadWriteLock = new ReaderWriterLockSlim();
    }

    public ServerResult<Player> RegisterPlayer(in PlayerInfo playerInfo)
    {
        using ScopedUpgradeableLock upgradeScope = ReadWriteLock.EnterUpgradeableScope();

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

        using (ReadWriteLock.EnterWriteScope())
        {
            _players[index] = player;
        }

        return ServerResult<Player>.Success(player);
    }

    public ServerResult UnregisterPlayer(Player player)
    {
        using ScopedUpgradeableLock upgradeableScope = ReadWriteLock.EnterUpgradeableScope();

        for (int i = 0; i < _players.Length; i++)
        {
            if (_players[i] == player)
            {
                using (ReadWriteLock.EnterWriteScope())
                {
                    _players[i] = null!;
                }

                return ServerResult.Success();
            }
        }

        return ServerResult.Failure(ServerError.OperationFailed);
    }

    public Player? FindPlayerById(PlayerId id)
    {
        using ScopedReadLock readScope = ReadWriteLock.EnterReadScope();

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
        using ScopedReadLock readScope = ReadWriteLock.EnterReadScope();

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

        using ScopedReadLock readScope = ReadWriteLock.EnterReadScope();

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
