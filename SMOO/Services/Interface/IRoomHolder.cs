using System.Net;
using SMOO.Client;
using SMOO.Server;

namespace SMOO.Services.Interface;

/// <summary>
/// Holds a set of rooms, allowing for adding, removing, and retrieving rooms by their ID. It also provides functionality to find players by their host endpoint and to shut down all rooms.
/// </summary>
internal interface IRoomHolder
{
    /// <summary>
    /// Adds a new room to the holder and returns the created room instance.
    /// </summary>
    /// <param name="context">The context for the new room</param>
    /// <returns>The created room instance</returns>
    IRoom AddRoom(ServerContext context);

    /// <summary>
    /// Removes a room from the holder by its ID. Returns true if the room was successfully removed, false if the room was not found.
    /// </summary>
    /// <param name="id">The ID of the room to remove</param>
    /// <returns>True if the room was successfully removed, false if the room was not found</returns>
    Task<bool> RemoveRoom(ushort id);

    /// <summary>
    /// Retrieves a room by its ID. Returns the room instance if found, or null if the room does not exist.
    /// </summary>
    /// <param name="id">The ID of the room to retrieve</param>
    /// <returns>The room instance if found, or null if the room does not exist</returns>
    IRoom? GetRoom(ushort id);

    /// <summary>
    /// Finds a player by their host endpoint. Returns the player instance if found, or null if no player with the specified endpoint exists.
    /// </summary>
    /// <param name="endpoint">The host endpoint of the player to find</param>
    /// <returns>The player instance if found, or null if no player with the specified endpoint exists</returns>
    Player? FindPlayerByHost(IPEndPoint endpoint);

    /// <summary>
    /// Shuts down all rooms managed by the holder, ensuring that each room is properly cleaned up and any necessary resources are released.
    /// </summary>
    /// <returns>A task that waits for all rooms to be shut down</returns>
    Task ShutdownRooms();

    /// <summary>
    /// Retrieves all rooms managed by the holder. Returns an enumerable collection of room instances.
    /// </summary>
    /// <returns>An enumerable collection of rooms in the holder</returns>
    IEnumerable<IRoom> GetRooms();
}
