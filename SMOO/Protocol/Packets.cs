using SMOO.Attributes;
using SMOO.Client;
using SMOO.Enumerator;
using SMOO.Serialization;
using SMOO.Server;
using SMOO.Memory;

namespace SMOO.Protocol;

/// <summary>
/// The packet sent to a player that just connected to a room
/// </summary>
internal ref struct PacketConnectSynAck : ISerializableStruct
{
    public required PacketHeader Header;
    public required Guid SessionId;
    public required byte RoomSize;
    public required byte SelfSlot;
    public required byte OtherPlayersCount;
    public required PlayerInRoomInfoEnumerator PlayerInfos;

    public readonly void Serialize(ref SpanWriter writer)
    {
        writer.Write(Header);
        writer.Write(SessionId);
        writer.Write(RoomSize);
        writer.Write(SelfSlot);
        writer.Write(OtherPlayersCount);

        foreach (PlayerInRoomInfo playerInfo in PlayerInfos)
        {
            playerInfo.Serialize(ref writer);
        }
    }
}

/// <summary>
/// The packet sent to acknowledge a reliable server packet
/// </summary>
internal ref struct PacketAck : ISerializableStruct
{
    [RequiredField]
    public PacketHeader Header;

    public readonly void Serialize(ref SpanWriter writer)
    {
        writer.Write(Header);
    }
}

/// <summary>
/// The packet sent to a room, to notify that a new player has joined
/// </summary>
internal ref struct PacketPlayerJoinRoom : ISerializableStruct
{
    [RequiredField]
    public required PacketHeader Header;

    [RequiredField]
    public required PlayerInRoomInfo PlayerRoomInfo;

    public PacketPlayerJoinRoom()
    {
        PlayerRoomInfo = new PlayerInRoomInfo();
    }

    public readonly void Serialize(ref SpanWriter writer)
    {
        writer.Write(Header);

        PlayerRoomInfo.Serialize(ref writer);
    }
}

/// <summary>
/// The packet broadcasted to a room when a player sends a chat message
/// </summary>
internal ref struct PacketChatMessage : ISerializableStruct
{
    [RequiredField]
    public required PacketHeader Header;

    [RequiredField]
    public required byte PlayerSlot;

    [DynamicField(MaxSize = Constants.MaxChatMessageLength)]
    public required StreamStringView<ushort> Message;

    public readonly void Serialize(ref SpanWriter writer)
    {
        writer.Write(Header);
        writer.Write(PlayerSlot);

        Message.Serialize(ref writer);
    }
}

/// <summary>
/// The packet sent to a player that just joined a stage
/// </summary>
internal ref struct PacketPlayersInStage : ISerializableStruct
{
    [RequiredField]
    public required PacketHeader Header;

    [RequiredField]
    public required byte PlayerCount;

    [DynamicRepeatedField(Type = typeof(PlayerInStageInfo), MaxRepeatCount = Constants.MaxRoomSize)]
    public required PlayerSameStageEnumerator PlayersInStage;

    internal ref struct PlayerInStageInfo : ISerializableStruct
    {
        [RequiredField]
        public byte PlayerSlot;

        [DynamicField(MaxSize = Constants.MaxAnimNameLength)]
        public StreamStringView<byte> Anim;

        [DynamicField(MaxSize = Constants.MaxAnimNameLength)]
        public StreamStringView<byte> SubAnim;

        [DynamicField(MaxSize = Constants.MaxAnimNameLength)]
        public StreamStringView<byte> UpperAnim;

        public PlayerInStageInfo(Player player)
        {
            PlayerSlot = player.Slot;

            Anim = new StreamStringView<byte>(player.SyncData.Anim);
            SubAnim = new StreamStringView<byte>(player.SyncData.SubAnim);
            UpperAnim = new StreamStringView<byte>(player.SyncData.UpperAnim);
        }

        public readonly void Serialize(ref SpanWriter writer)
        {
            writer.Write(PlayerSlot);

            Anim.Serialize(ref writer);
            SubAnim.Serialize(ref writer);
            UpperAnim.Serialize(ref writer);
        }
    }

    public readonly void Serialize(ref SpanWriter writer)
    {
        writer.Write(Header);
        writer.Write(PlayerCount);

        foreach (Player player in PlayersInStage)
        {
            PlayerInStageInfo stageInfo = new PlayerInStageInfo(player);

            stageInfo.Serialize(ref writer);
        }
    }
}
