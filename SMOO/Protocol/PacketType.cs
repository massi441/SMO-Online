namespace SMOO.Protocol;

internal enum PacketType : byte
{
    ConnectSyn,
    ConnectSynAck,
    ConnectAck,
    Disconnect,
    PlayerJoinRoom,
    HealthCheck,
    Ping,
    Ack,
    ChatMessage,
    ChatMessageRequest,
    Event,
    PlayersInStage,

    /// <summary>
    /// A reserved packet type for server side validation
    /// </summary>
    OutOfRange
}
