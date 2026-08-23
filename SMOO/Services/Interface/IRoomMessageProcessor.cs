using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Services.Interface;

/// <summary>
/// Represents a handler for a specific room message
/// </summary>
internal interface IRoomMessageProcessor
{
    void Process(Room room, Packet packet);
}
