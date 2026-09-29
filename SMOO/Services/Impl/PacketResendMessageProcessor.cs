using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Interface;

namespace SMOO.Services.Impl;

/// <summary>
/// Resends sequenced packets if they have not been acknowledged after a certain delay
/// </summary>
internal class PacketResendMessageProcessor : IRoomMessageProcessor
{
    private readonly ServerContext _context;

    public PacketResendMessageProcessor(ServerContext context)
    {
        _context = context;
    }

    public void Process(Room room, NetworkPacket packet)
    {
        //_context.Logger.LogTrace("Packet resend message processor invoked in Room #{RoomId}", room.Id);

        foreach (Player player in room.Players)
        {
            player.SequencedPacketStore.ResendPackets();
        }
    }
}
