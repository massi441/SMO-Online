using Microsoft.Extensions.Logging;
using SMOO.Attributes;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Serialization;
using SMOO.Server;
using SMOO.Memory;

namespace SMOO.Event;

internal class EventChangeCapHandler : IEventHandler
{
    public static ushort MinDataSize => RequiredSize<ChangeCapData>.MinSize;

    public static ushort MaxDataSize => RequiredSize<ChangeCapData>.MaxSize;

    private ref struct ChangeCapData : IDeserializableStruct
    {
        [DynamicField(MaxSize = Constants.MaxCostumeNameLength)]
        public StreamStringView<byte> CapName;

        public void Deserialize(ref SpanReader reader)
        {
            CapName.Deserialize(ref reader, Constants.MaxCostumeNameLength);
        }
    }

    public static void Handle(ParsedEventPacket packet, Room room, ServerContext context)
    {
        Player player = packet.BasePacket.SenderPlayer!;

        try
        {
            ChangeCapData capData = PacketSerializer.Deserialize<ChangeCapData>(packet.EventData);

            if (!capData.CapName.HasData())
            {
                PacketUtil.AckEvent(packet, context);
                context.Logger.LogError("{PlayerName} sent an empty cap name in Room #{RoomId}", player.Name, room.Id);
                return;
            }

            if (player.WorldInfo.CostumeCap == capData.CapName.String)
            {
                PacketUtil.AckEvent(packet, context);
                context.Logger.LogInformation("{PlayerName} is already wearing the {CapName} cap, broadcast will be skipped", player.Name, capData.CapName);
                return;
            }

            player.WorldInfo.CostumeCap = capData.CapName.String;

            context.Logger.LogInformation("{PlayerName} is now wearing cap {CapName} in Room #{RoomId}", player.Name, capData.CapName, room.Id);

            PacketUtil.AckEvent(packet, context);

            room.Broadcaster.BroadcastReliably(packet.BasePacket.Buffer, room.Players.Except(player));
        }
        catch (InvalidDataException ex)
        {
            PacketUtil.AckEvent(packet, context);
            context.Logger.LogInformation("{PlayerName} sent an invalid change cap packet in Room #{RoomId}: {Message}", player.Name, room.Id, ex.Message);
        }
    }
}
