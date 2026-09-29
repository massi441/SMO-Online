using Microsoft.Extensions.Logging;

namespace SMOO.Server;

/// <summary>
/// The server configuration settings.
/// </summary>
internal record class ServerConfig
{
    /// <summary>
    /// The port the server should listen on. Default is 5001.
    /// </summary>
    public int Port { get; init; } = 5001;

    /// <summary>
    /// The log level for the server. Default is LogLevel.Trace.
    /// </summary>
    public LogLevel LogLevel { get; init; } = LogLevel.Trace;
}
