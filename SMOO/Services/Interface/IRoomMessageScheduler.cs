using SMOO.Services.Impl;

namespace SMOO.Services.Interface;

/// <summary>
/// Perdiodically sends messages to a room that can be processed by their respective room message processor
/// </summary>
internal interface IRoomMessageScheduler
{
    /// <summary>
    /// Initializes the scheduler with the room it will be sending messages to
    /// </summary>
    /// <param name="room">The room to which messages will be sent</param>
    void Start(Room room);

    /// <summary>
    /// Shuts down the scheduler, stopping any ongoing message sending and cleaning up resources
    /// </summary>
    /// <returns>A task that waits for the shutdown to complete</returns>
    Task Shutdown();
}
