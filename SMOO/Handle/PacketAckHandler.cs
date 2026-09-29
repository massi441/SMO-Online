using SMOO.Protocol;
using SMOO.Server;
using Microsoft.Extensions.Logging;
using SMOO.Services.Impl;

namespace SMOO.Handle;

internal class PacketAckHandler : IPacketHandler
{
    public static ushort MinPayloadSize => 0;
    public static ushort MaxPayloadSize => 0;

    public static void Handle(RoomPacket packet, Room room, ServerContext context)
    {
        ushort sequenceNumber = packet.Header.SequenceNumber;

        SequencedPacket? pendingPacket = packet.SenderPlayer!.SequencedPacketStore.RemovePacket(sequenceNumber);
        if (pendingPacket == null)
        {
            context.Logger.LogWarning("The packet #{SequenceNumber} was not found in room #{RoomId}, likely already Acked", sequenceNumber, room.Id);
            return;
        }

        context.Logger.LogTrace("Successfully Acked packet #{PacketNumber} from {PlayerName} in Room #{RoomId}", pendingPacket.SequenceNumber, pendingPacket.Receiver.Name, room.Id);
    }
}
