using SMOO.Client;
using SMOO.Server;

namespace SMOO.Services.Interface;

/// <summary>
/// Represents a room in the SMOO server, which holds players and manages their interactions
/// </summary>
internal interface IRoom
{
    /// <summary>
    /// The unique identifier of the room
    /// </summary>
    int Id { get; }

    /// <summary>
    /// The player holder that manages the players in the room
    /// </summary>
    IPlayerHolder PlayerHolder { get; }

    /// <summary>
    /// The broadcaster that relays messages to the players in the room
    /// </summary>
    IBroadcaster Broadcaster { get; }

    /// <summary>
    /// Starts the room, initializing any necessary resources and starting any background tasks
    /// </summary>
    void Start();

    /// <summary>
    /// Shuts down the room, cleaning up any resources and stopping any background tasks
    /// </summary>
    /// <returns>A task that waits for the shutdown to complete</returns>
    Task Shutdown();

    /// <summary>
    /// Uploads a <see cref="RoomMessage"/> as work for the room to process
    /// </summary>
    /// <param name="roomMessage"></param>
    void UploadMessage(RoomMessage roomMessage);

    /// <summary>
    /// Requests the disconnection of a player from the room, removing them from the player holder and notifying other players in the room.
    /// If the player already had a disconnection request, the request is ignored.
    /// </summary>
    /// <param name="player">The player to disconnect</param>
    void RequestDisconnection(Player player);
}
