using System.Net;
using SMOO.Client;
using SMOO.Server;

namespace SMOO.Services.Interface;

internal interface IRoomHolder
{
    IRoom AddRoom(ServerContext context);
    Task<bool> RemoveRoom(ushort id);
    IRoom? GetRoom(ushort id);
    Player? FindPlayerByHost(IPEndPoint endpoint);
    Task ShutdownRooms();
    IEnumerable<IRoom> GetRooms();
}
