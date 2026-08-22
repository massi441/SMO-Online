using SMOO.Protocol;

namespace SMOO.Server;

// Potential future note: Add priority to message (Packets would be a lot higher)

/// <summary>
/// Represents a message that can be processed by a room
/// </summary>
internal readonly struct RoomMessage
{
    public required RoomMessageType Type { get; init; }
    public Packet? Packet { get; init; }
}
