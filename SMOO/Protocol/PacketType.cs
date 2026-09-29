namespace SMOO.Protocol;

/// <summary>
/// Represents a type of packet that can be sent or received
/// </summary>
internal enum PacketType : byte
{
    /// <summary>
    /// A client wants to initiate a connection handshake with the server
    /// </summary>
    ConnectSyn,

    /// <summary>
    /// A server wants to respond to a connection handshake with the client
    /// </summary>
    ConnectSynAck,

    /// <summary>
    /// A client wants to acknowledge and complete connection handshake with the server
    /// </summary>
    ConnectAck,

    /// <summary>
    /// A client wants to disconnect from the server
    /// </summary>
    Disconnect,

    /// <summary>
    /// A new player joined a room, and the server is broadcasting this information to all players in the room
    /// </summary>
    PlayerJoinRoom,

    /// <summary>
    /// A player has been idle for too long and needs a health check from the server
    /// </summary>
    HealthCheck,

    /// <summary>
    /// A ping to see if the server is up
    /// </summary>
    Ping,

    /// <summary>
    /// A sequenced packet needs to be acknowledged
    /// </summary>
    Ack,

    /// <summary>
    /// A chat message is being broadcasted to all players in the room
    /// </summary>
    ChatMessage,
    ChatMessageRequest, // TODO: Remove this and merge with single Chat Message type (when server receives it broadcasts it to all players)

    /// <summary>
    /// A game event has occurred
    /// </summary>
    Event,

    /// <summary>
    /// A packet that contains information about all players in the current stage of a player
    /// </summary>
    PlayersInStage,

    /// <summary>
    /// A reserved packet type for server side validation
    /// </summary>
    OutOfRange
}
