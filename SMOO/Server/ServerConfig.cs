using Microsoft.Extensions.Logging;

namespace SMOO.Server;

internal record class ServerConfig
{
    public int Port { get; init; } = Constants.DefaultPort;
    public LogLevel LogLevel { get; init; } = Constants.DefaultLogLevel;
}
