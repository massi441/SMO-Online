using Microsoft.Extensions.Logging;
using SMOO.Attributes;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Serialization;
using SMOO.Server;
using SMOO.MemUtil;

namespace SMOO.Event;

internal class EventChangeCostumeHandler : IEventHandler
{
    public static ushort MinDataSize => RequiredSize<ChangeCostumeData>.MinSize;
    public static ushort MaxDataSize => RequiredSize<ChangeCostumeData>.MaxSize;

    private ref struct ChangeCostumeData : IDeserializableStruct
    {
        [DynamicField(MaxSize = Constants.MaxCostumeNameLength)]
        public StreamStringView<byte> CostumeName;

        public void Deserialize(ref SpanReader reader)
        {
            CostumeName.Deserialize(ref reader, Constants.MaxCostumeNameLength);
        }
    }

    public static void Handle(ParsedEventPacket packet, Room room, ServerContext context)
    {
        Player player = packet.BasePacket.SenderPlayer!;

        try
        {
            ChangeCostumeData costumeData = PacketSerializer.Deserialize<ChangeCostumeData>(packet.EventData);

            if (!costumeData.CostumeName.HasData())
            {
                PacketUtil.AckEvent(packet, context);
                context.Logger.LogError("{PlayerName} sent an empty costume name in Room #{RoomId}", player.Name, room.Id);
                return;
            }

            if (player.WorldInfo.CostumeBody == costumeData.CostumeName.String)
            {
                PacketUtil.AckEvent(packet, context);
                context.Logger.LogInformation("{PlayerName} is already wearing the {CapName} costume, broadcast will be skipped", player.Name, costumeData.CostumeName);
                return;
            }

            player.WorldInfo.CostumeBody = costumeData.CostumeName.String;

            context.Logger.LogInformation("{PlayerName} is now wearing costume {CostumeName} in Room #{RoomId}", player.Name, costumeData.CostumeName, room.Id);

            PacketUtil.AckEvent(packet, context);

            room.Broadcaster.BroadcastReliably(packet.BasePacket.Buffer, room.Players.Except(player));
        }
        catch (InvalidDataException ex)
        {
            PacketUtil.AckEvent(packet, context);
            context.Logger.LogInformation("{PlayerName} sent an invalid change costume packet in Room #{RoomId}: {Message}", player.Name, room.Id, ex.Message);
        }
    }
}
