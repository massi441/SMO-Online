using System.Net;
using SMOO.Client;
using SMOO.Server;

namespace SMOO.Services.Interface;

internal interface IRoomHolder
{
    ushort AddRoom(ServerContext context);
    Task<bool> RemoveRoom(ushort id);
    Room? GetRoom(ushort id);
    Player? FindPlayerByHost(IPEndPoint endpoint);
    Task ShutdownRooms();
    IEnumerable<Room> GetRooms();
}
