using SMOO.Client;
using SMOO.Server;

namespace SMOO.Services.Interface;

/// <summary>
/// Disconnects a player from a room
/// </summary>
internal interface IPlayerDisconnector
{
    ServerResult Disconnect(Player player);
}
