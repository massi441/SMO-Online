using Microsoft.Extensions.Logging;
using SMOO.Attributes;
using SMOO.Protocol;
using SMOO.Serialization;
using SMOO.Server;
using SMOO.MemUtil;

namespace SMOO.Handle;

internal class PacketChatMessageHandler : IPacketHandler
{
    /// <summary>
    /// Requires one UInt16 for the length of the message
    /// </summary>
    public static ushort MinPayloadSize => RequiredSize<PacketChatMessageRequest>.MinSize;
    public static ushort MaxPayloadSize => RequiredSize<PacketChatMessageRequest>.MaxSize;

    private struct PacketChatMessageRequest : IDeserializableStruct
    {
        [DynamicField(MaxSize = Constants.MaxChatMessageLength)]
        public StreamStringView<ushort> Message;

        public void Deserialize(ref SpanReader reader)
        {
            Message.Deserialize(ref reader, Constants.MaxChatMessageLength);
        }
    }

    public static void Handle(ParsedPacket packet, Room room, ServerContext context)
    {
        PacketChatMessageRequest request = PacketSerializer.Deserialize<PacketChatMessageRequest>(packet.Payload);

        context.Logger.LogTrace("{PlayerName} sent a message in room #{RoomId}: {Message}", packet.SenderPlayer!.Name, room.Id, request.Message);

        PacketChatMessage chatPacket = new PacketChatMessage()
        {
            Header = packet.Header.WithType(PacketType.ChatMessage),
            PlayerSlot = packet.SenderPlayer!.Slot,
            Message = request.Message,
        };

        using SharedBuffer chatBuffer = PacketSerializer.SerializeShared(ref chatPacket, RequiredSize<PacketChatMessage>.MaxSize);

        room.Broadcaster.BroadcastReliably(chatBuffer, room.Players.Except(packet.SenderPlayer));
    }
}
