namespace SMOO.Event;

/// <summary>
/// Represents a event that occurs during a game session. These events are sent from the client to the server, and then broadcasted to all other clients in the same room.
/// </summary>
internal enum EventType : ushort
{
    /// <summary>
    /// The player has gone to another stage
    /// </summary>
    ChangeStage,

    /// <summary>
    /// The player has changed their costume
    /// </summary>
    ChangeCostume,

    /// <summary>
    /// The player has changed their cap
    /// </summary>
    ChangeCap,

    /// <summary>
    /// The player has sent a game sync packet
    /// </summary>
    GameSync,
}
