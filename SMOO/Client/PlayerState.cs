namespace SMOO.Client;


/// <summary>
/// The state a player can be in
/// </summary>
internal enum PlayerState : byte
{
    /// <summary>
    /// The player has initated a connection handshake to the server
    /// </summary>
    Connecting,

    /// <summary>
    /// The player has completed the handshake with the server
    /// </summary>
    Connected,

    /// <summary>
    /// The player will be disconnected from the server
    /// </summary>
    Disconnecting
}
