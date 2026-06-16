using SMOO.Client;
using SMOO.Server;

namespace SMOO.Services.Interface;

internal interface IPlayerDisconnector
{
    ServerResult Disconnect(Player player);
}
