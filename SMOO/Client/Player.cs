
using System.Net;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Client;

internal class Player
{
    public required PlayerId Id { get; init; }
    public required string Name { get; init; }
    public required PlayerWorldInfo WorldInfo { get; init; }
    public required PlayerSyncData SyncData { get; init; }
    public required Room Room { get; init; }
    public required byte Slot { get; init; }
    public DateTime LastSeen { get; private set; } = DateTime.UtcNow;
    public IPEndPoint Endpoint => Id.Endpoint;

    public void RefreshLastSeen()
    {
        LastSeen = DateTime.UtcNow;
    }

    public bool IsConnectionLost()
    {
        return (DateTime.UtcNow - LastSeen).TotalMilliseconds > Config.PlayerConnectionLostThreshold;
    }

    public bool IsNeedHealthCheck()
    {
        return (DateTime.UtcNow - LastSeen).TotalMilliseconds > Config.PlayerHealthCheckThreshold;
    }
}
