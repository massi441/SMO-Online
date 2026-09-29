using SMOO.Protocol;
using SMOO.Services.Impl;

namespace SMOO.Services.Interface;

/// <summary>
/// Represents a handler for a specific room message
/// </summary>
internal interface IRoomMessageProcessor
{
    /// <summary>
    /// Processes messages received in a room, handling the logic for each message type and updating the room state accordingly
    /// </summary>
    /// <param name="room">The room in which the message was received</param>
    /// <param name="packet">The network packet containing the message</param>
    void Process(Room room, NetworkPacket packet);
}
