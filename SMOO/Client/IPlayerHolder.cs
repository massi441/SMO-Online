using System.Net;
using SMOO.Server;

namespace SMOO.Client;

internal interface IPlayerHolder
{
    PlayerList Players { get; }
    byte MaxSize { get; }
    ReaderWriterLockSlim ReadWriteLock { get; }
    ServerResult<Player> RegisterPlayer(in PlayerInfo playerInfo);
    ServerResult UnregisterPlayer(Player player);
    Player? FindPlayerByHost(IPEndPoint endpoint);
    Player? FindPlayerById(PlayerId id);
}
