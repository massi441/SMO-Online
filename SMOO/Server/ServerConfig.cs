using Microsoft.Extensions.Logging;

namespace SMOO.Server;

internal record class ServerConfig
{
    public int Port { get; init; } = 5001;
    public LogLevel LogLevel { get; init; } = LogLevel.Trace;
}
