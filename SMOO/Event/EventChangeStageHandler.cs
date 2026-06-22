using Microsoft.Extensions.Logging;
using SMOO.Attributes;
using SMOO.Client;
using SMOO.Enumerator;
using SMOO.Protocol;
using SMOO.Serialization;
using SMOO.Server;
using SMOO.MemUtil;

namespace SMOO.Event;

internal class EventChangeStageHandler : IEventHandler
{
    public static ushort MinDataSize => RequiredSize<ChangeStageData>.MinSize;
    public static ushort MaxDataSize => RequiredSize<ChangeStageData>.MaxSize;

    private struct ChangeStageData : IDeserializableStruct
    {
        [RequiredField]
        public byte ScenarioId;

        [DynamicField(MaxSize = Constants.MaxStageNameLength)]
        public StreamStringView<byte> NewStage; // string.Empty is the "Left" stage signal

        public void Deserialize(ref SpanReader reader)
        {
            ScenarioId = reader.ReadByte();
            NewStage.Deserialize(ref reader, Constants.MaxStageNameLength);
        }
    }

    public static void Handle(ParsedEventPacket packet, Room room, ServerContext context)
    {
        ChangeStageData data = PacketSerializer.Deserialize<ChangeStageData>(packet.EventData);

        Player player = packet.BasePacket.SenderPlayer!;
        string previousPlayerStage = player.WorldInfo.CurrentStage;

        player.WorldInfo.CurrentStage = data.NewStage.String;

        PacketUtil.AckEvent(packet, context); // need to ack before the broadcast overwrites the sequence number

        room.Broadcaster.BroadcastReliably(packet.BasePacket.Buffer, room.Players.Except(player));

        if (data.NewStage.String.Length > 0)
        {
            context.Logger.LogInformation("Player {PlayerName} changed to stage {StageName} (Scenario {ScenarioId}) in Room #{RoomId}", player.Name, data.NewStage, data.ScenarioId, player.Room.Id);

            PlayerSameStageEnumerator playersInStage = room.Players.SameStageAs(player);

            byte inStageCount = (byte)playersInStage.Count<Player, PlayerSameStageEnumerator>();

            if (inStageCount > 0)
            {
                PacketPlayersInStage playersInStagePacket = new PacketPlayersInStage()
                {
                    Header = packet.BasePacket.Header.WithType(PacketType.PlayersInStage),
                    PlayerCount = inStageCount,
                    PlayersInStage = playersInStage
                };

                using SharedBuffer buffer = PacketSerializer.SerializeShared(ref playersInStagePacket, RequiredSize<PacketPlayersInStage>.MaxSize);

                context.Logger.LogInformation("{PlayerCount} players were already in stage {StageName}, {PlayerName} will be notified", inStageCount, data.NewStage, player.Name);

                context.PacketSender.SendReliably(buffer, player, room.ReliableStore);
            }
        }
        else
        {
            context.Logger.LogInformation("Player {PlayerName} left stage {StageName} in Room #{RoomId}", player.Name, previousPlayerStage, player.Room.Id);
        }
    }
}
