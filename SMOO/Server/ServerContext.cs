using Microsoft.Extensions.Logging;
using SMOO.Services.Interface;

namespace SMOO.Server;

/// <summary>
/// The context object containing services needed to run the server
/// </summary>
internal class ServerContext
{
    /// <summary>
    /// The logger used across the server
    /// </summary>
    public required ILogger Logger { get; init; }

    /// <summary>
    /// The room holder used across the server
    /// </summary>
    public required IRoomHolder RoomHolder { get; init; }

    /// <summary>
    /// The packet sender used across the server
    /// </summary>
    public required IPacketController PacketController { get; init; }

    /// <summary>
    /// The cancellation used to signal a server shutdown
    /// </summary>
    public required CancellationToken CancellationToken { get; init; }
}
