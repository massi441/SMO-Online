namespace SMOO.Server;

/// <summary>
/// Represents a type of "Message" that a room can process
/// </summary>
internal enum RoomMessageType : byte
{
    /// <summary>
    /// A netowrk packet needs to be processed
    /// </summary>
    Packet,

    /// <summary>
    /// Reliable packets need to be checked and resent if needed
    /// </summary>
    PacketResend,

    /// <summary>
    /// Player idle statuses need to be checked
    /// </summary>
    PlayerHealthCheck,
}
