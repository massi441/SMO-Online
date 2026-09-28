using Core.Memory;
using Microsoft.Extensions.Logging;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Impl;

namespace SMOO.Handle;

/// <summary>
/// The packet handler that completes the connection handshake with the client
/// </summary>
internal class PacketConnectAckHandler : IPacketHandler
{
    public static ushort MinPayloadSize => 0;
    public static ushort MaxPayloadSize => 0;

    public static void Handle(ParsedPacket packet, Room room, ServerContext context)
    {
        ushort sequenceNumber = packet.Header.SequenceNumber;

        SequencedPacket? ackPacket = packet.SenderPlayer!.SequencedPacketStore.RemovePacket(sequenceNumber);
        if (ackPacket == null)
        {
            context.Logger.LogWarning("Invalid SYN ACK sequence number ({SequenceNumber}) received by {PlayerName} in Room #{RoomId}, broadcast will be skipped", sequenceNumber, packet.SenderPlayer?.Name, room.Id);
            return;
        }

        packet.SenderPlayer!.MarkConnected();

        PacketPlayerJoinRoom joinPacket = new PacketPlayerJoinRoom()
        {
            Header = packet.Header.WithType(PacketType.PlayerJoinRoom),
            PlayerRoomInfo = new PlayerInRoomInfo(packet.SenderPlayer!)
        };

        using RentedBuffer joinRoomBuffer = PacketSerializer.SerializeShared(ref joinPacket, RequiredSize<PacketPlayerJoinRoom>.MaxSize);

        context.Logger.LogInformation("Player {PlayerName} has confirmed their connection in Room #{RoomId}, room will be notified", packet.SenderPlayer!.Name, room.Id);

        room.Broadcaster.BroadcastReliably(joinRoomBuffer, room.Players.Except(packet.SenderPlayer)); // transfers ownership of the rented buffer to the sequenced stores
    }
}
