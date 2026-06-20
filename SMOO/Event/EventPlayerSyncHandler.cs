using System.Numerics;
using Microsoft.Extensions.Logging;
using SMOO.Attributes;
using SMOO.Client;
using SMOO.Enumerator;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Event;

internal class EventPlayerSyncHandler : IEventHandler
{
    public static ushort MinDataSize => (ushort)(RequiredSize<PlayerSyncData>.MinSize + RequiredSize<CapSyncData>.MinSize);
    public static ushort MaxDataSize => (ushort)(RequiredSize<PlayerSyncData>.MaxSize + RequiredSize<CapSyncData>.MaxSize);

    private ref struct PlayerSyncData : IDeserializableStruct
    {
        [RequiredField]
        public Vector3 Position;

        [RequiredField]
        public Quaternion Quat;

        [RequiredField]
        public float AnimRate;

        [DynamicField(MaxSize = Config.MaxAnimNameLength)]
        public StreamStringView<byte> Anim;
        
        [DynamicField(MaxSize = Config.MaxAnimNameLength)]
        public StreamStringView<byte> SubAnim;
        
        [DynamicField(MaxSize = Config.MaxAnimNameLength)]
        public StreamStringView<byte> UpperAnim;
        
        [DynamicField(MaxSize = Config.MaxBlendWeights * sizeof(float))]
        public StreamSpanView<byte, float> BlendWeights;

        public void Deserialize(ref SpanReader reader)
        {
            reader.ReadInto(ref Position);
            reader.ReadInto(ref Quat);

            AnimRate = reader.ReadSingleLittleEndian();

            Anim.Deserialize(ref reader, Config.MaxAnimNameLength);
            SubAnim.Deserialize(ref reader, Config.MaxAnimNameLength);
            UpperAnim.Deserialize(ref reader, Config.MaxAnimNameLength);
            BlendWeights.Deserialize(ref reader, Config.MaxBlendWeights);
        }
    }

    private ref struct CapSyncData : IDeserializableStruct
    {
        [RequiredField]
        public Vector3 Position;

        [RequiredField]
        public Quaternion Quat;

        [DynamicField(MaxSize = Config.MaxAnimNameLength)]
        StreamStringView<byte> Anim;

        [RequiredField]
        public Vector3 JointKeeperRotation;

        [RequiredField]
        public float JointKeeperSkew;

        [RequiredField]
        public bool IsAlive;

        public void Deserialize(ref SpanReader reader)
        {
            reader.ReadInto(ref Position);
            reader.ReadInto(ref Quat);

            Anim.Deserialize(ref reader, Config.MaxAnimNameLength);

            reader.ReadInto(ref JointKeeperRotation);
            reader.ReadInto(ref JointKeeperSkew);
            reader.ReadInto(ref IsAlive);
        }
    }

    public static void Handle(ParsedEventPacket eventPacket, Room room, ServerContext context)
    {
        try
        {
            SpanReader reader = new SpanReader(eventPacket.EventData);

            PlayerSyncData playerSyncData = PacketSerializer.Deserialize<PlayerSyncData>(ref reader);
            CapSyncData capSyncData = PacketSerializer.Deserialize<CapSyncData>(ref reader);

            Player player = eventPacket.BasePacket.SenderPlayer!;

            if (playerSyncData.Anim.HasData())
            {
                player.SyncData.Anim = playerSyncData.Anim.String; 
            }

            if (playerSyncData.SubAnim.HasData())
            {
                player.SyncData.SubAnim = playerSyncData.SubAnim.String;
            }

            if (playerSyncData.UpperAnim.HasData())
            {
                player.SyncData.UpperAnim = playerSyncData.UpperAnim.String;
            }

            PlayerSameStageEnumerator playersInStage = room.Players.SameStageAs(player);

            room.Broadcaster.Broadcast(eventPacket.BasePacket.Buffer, playersInStage);
        }
        catch (InvalidDataException ex)
        {
            context.Logger.LogError("{PlayerName} sent a malformed player sync payload in Room #{RoomId}: {Message}", eventPacket.BasePacket.SenderPlayer!.Name, room.Id, ex.Message);
        }
    }
}
