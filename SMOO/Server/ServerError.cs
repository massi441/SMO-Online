namespace SMOO.Server;

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
