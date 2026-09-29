using SMOO.Server;

namespace SMOO.Services.Interface;

/// <summary>
/// Holds a list of <see cref="IRoomMessageProcessor"/> instances, allowing retrieval of the appropriate processor for a given <see cref="RoomMessageType"/>
/// </summary>
internal interface IRoomMessageProcessorList
{
    /// <summary>
    /// Retrieves the appropriate <see cref="IRoomMessageProcessor"/> for the specified <see cref="RoomMessageType"/>
    /// </summary>
    /// <param name="type">The type of room message for which to retrieve a processor</param>
    /// <returns>The appropriate <see cref="IRoomMessageProcessor"/> instance</returns>
    IRoomMessageProcessor GetProcessor(RoomMessageType type);
}
