using System.Net;
using SMOO.Client;
using SMOO.Server;
using SMOO.Services.Interface;

namespace SMOO.Services.Impl;

internal class RoomHolder : IRoomHolder
{
    private readonly Dictionary<ushort, Room> _rooms = [];

    public Room AddRoom(ServerContext context)
    {
        ushort nextId = 0;

        if (_rooms.Count > 0)
        {
            nextId = (ushort)(_rooms.Keys.Max() + 1); 
        }

        // Generic services
        PlayerHolder playerHolder = new PlayerHolder();
        ReliablePacketStore reliablePacketStore = new ReliablePacketStore(context);
        Broadcaster roomBroadcaster = new Broadcaster(context, reliablePacketStore);

        // Room services (IRoomService and IRoomServiceList)
        PacketMessageProcessor packetProcessor = new PacketMessageProcessor(context);
        PacketResendMessageProcessor packetResendProcessor = new PacketResendMessageProcessor(context, reliablePacketStore);
        PlayerHealthMessageProcessor playerHealthProcessor = new PlayerHealthMessageProcessor(context, playerHolder);

        RoomMessageProcessorList roomServices = new RoomMessageProcessorList(packetProcessor, packetResendProcessor, playerHealthProcessor);

        RoomMessageScheduler messageScheduler = new RoomMessageScheduler(context);

        Room room = new Room(nextId, context, playerHolder, roomBroadcaster, roomServices, messageScheduler);

        _rooms.Add(nextId, room);

        return room;
    }

    public async Task<bool> RemoveRoom(ushort id)
    {
        if (_rooms.TryGetValue(id, out Room? room))
        {
            if (room != null)
            {
                await room.Shutdown();
                _rooms.Remove(id);
                return true;
            }
        }

        return false;
    }

    public Room? GetRoom(ushort id)
    {
        if (_rooms.TryGetValue(id, out Room? room))
        {
            return room;
        }

        return null;
    }

    public Task ShutdownRooms()
    {
        return Task.WhenAll(_rooms.Values.Select(room => room.Shutdown()));
    }

    public IEnumerable<Room> GetRooms()
    {
        return _rooms.Values;
    }

    public Player? FindPlayerByHost(IPEndPoint endpoint)
    {
        foreach (Room room in _rooms.Values)
        {
            Player? player = room.PlayerHolder.FindPlayerByHost(endpoint);
            if (player != null)
            {
                return player;
            }
        }

        return null;
    }
}
