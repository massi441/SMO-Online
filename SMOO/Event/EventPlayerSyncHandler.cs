using System.Numerics;
using Microsoft.Extensions.Logging;
using SMOO.Attributes;
using SMOO.Client;
using SMOO.Enumerator;
using SMOO.Protocol;
using SMOO.Serialization;
using SMOO.Server;
using SMOO.Memory;

namespace SMOO.Event;

internal class EventPlayerSyncHandler : IEventHandler
{
    public static ushort MinDataSize => RequiredSize<SyncEventData>.MinSize;
    public static ushort MaxDataSize => RequiredSize<SyncEventData>.MaxSize;

    private ref struct PlayerSyncData : IDeserializableStruct
    {
        [RequiredField]
        public Vector3 Position;

        [RequiredField]
        public Quaternion Quat;

        [RequiredField]
        public float AnimRate;

        [DynamicField(MaxSize = Constants.MaxAnimNameLength)]
        public StreamStringView<byte> Anim;
        
        [DynamicField(MaxSize = Constants.MaxAnimNameLength)]
        public StreamStringView<byte> SubAnim;
        
        [DynamicField(MaxSize = Constants.MaxAnimNameLength)]
        public StreamStringView<byte> UpperAnim;
        
        [DynamicField(MaxSize = Constants.MaxBlendWeights * sizeof(float))]
        public StreamSpanView<byte, float> BlendWeights;

        public void Deserialize(ref SpanReader reader)
        {
            reader.ReadInto(ref Position);
            reader.ReadInto(ref Quat);

            AnimRate = reader.ReadSingleLittleEndian();

            Anim.Deserialize(ref reader, Constants.MaxAnimNameLength);
            SubAnim.Deserialize(ref reader, Constants.MaxAnimNameLength);
            UpperAnim.Deserialize(ref reader, Constants.MaxAnimNameLength);
            BlendWeights.Deserialize(ref reader, Constants.MaxBlendWeights);
        }
    }

    private ref struct CapSyncData : IDeserializableStruct
    {
        [RequiredField]
        public Vector3 Position;

        [RequiredField]
        public Quaternion Quat;

        [DynamicField(MaxSize = Constants.MaxAnimNameLength)]
        StreamStringView<byte> Anim;

        [RequiredField]
        public Vector3 SpinRotation;

        [RequiredField]
        public bool IsVisible;

        public void Deserialize(ref SpanReader reader)
        {
            reader.ReadInto(ref Position);
            reader.ReadInto(ref Quat);

            Anim.Deserialize(ref reader, Constants.MaxAnimNameLength);

            reader.ReadInto(ref SpinRotation);
            reader.ReadInto(ref IsVisible);
        }
    }

    private ref struct SyncEventData : IDeserializableStruct
    {
        [RequiredField]
        public int Frame;

        [RequiredField]
        public PlayerSyncData PlayerSyncData;

        [RequiredField]
        public CapSyncData CapSyncData;

        public void Deserialize(ref SpanReader reader)
        {
            Frame = reader.ReadInt32LittleEndian();

            PlayerSyncData.Deserialize(ref reader);
            CapSyncData.Deserialize(ref reader);
        }
    }

    public static void Handle(ParsedEventPacket eventPacket, Room room, ServerContext context)
    {
        try
        {
            SpanReader reader = new SpanReader(eventPacket.EventData);

            SyncEventData syncData = PacketSerializer.Deserialize<SyncEventData>(ref reader);

            Player player = eventPacket.BasePacket.SenderPlayer!;

            if (syncData.PlayerSyncData.Anim.HasData())
            {
                player.SyncData.Anim = syncData.PlayerSyncData.Anim.String; 
            }

            if (syncData.PlayerSyncData.SubAnim.HasData())
            {
                player.SyncData.SubAnim = syncData.PlayerSyncData.SubAnim.String;
            }

            if (syncData.PlayerSyncData.UpperAnim.HasData())
            {
                player.SyncData.UpperAnim = syncData.PlayerSyncData.UpperAnim.String;
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
