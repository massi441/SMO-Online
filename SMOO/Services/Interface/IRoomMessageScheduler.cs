using SMOO.Server;

namespace SMOO.Services.Interface;

/// <summary>
/// Perdiodically sends messages to a room, that can be processed 
/// by their respective room message processor
/// </summary>
internal interface IRoomMessageScheduler
{
    void Start(Room room);
    Task Shutdown();
}
