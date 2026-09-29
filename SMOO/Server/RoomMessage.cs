using SMOO.Protocol;

namespace SMOO.Server;

// Potential future note: Add priority to message (Packets would be a lot higher)

/// <summary>
/// Represents a message that can be processed by a room
/// </summary>
internal readonly struct RoomMessage
{
    /// <summary>
    /// The type of the message
    /// </summary>
    public required RoomMessageType Type { get; init; }

    /// <summary>
    /// The optional packet tied to the message, if any
    /// </summary>
    public NetworkPacket? Packet { get; init; }
}
