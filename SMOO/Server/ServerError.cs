namespace SMOO.Server;

/// <summary>
/// Represents various server error codes that can occur during packet handling, sending, room management, or anything else happening within the server.
/// </summary>
internal enum ServerError
{
    // Packet header
    InvalidMagic,
    InvalidHeaderSize,
    InvalidPacketType,
    InvalidVersion,

    // Packet Handling
    EmptyPayload,
    NoPacketHandler,
    InvalidNameLength,
    PayloadTooLarge,

    // Packet Sending
    NotSent,
    PendingPacketStoreFull,

    // Room
    RoomNotFound,
    RoomFull,
    PlayerAlreadyInRoom,
    IllegalRoomAccess,
    PlayerNotFound,

    // Generic
    OperationFailed,
    ConnectionLost
}
