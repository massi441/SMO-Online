using Microsoft.Extensions.Logging;

namespace SMOO.Server;

internal record class ServerConfig
{
    public int Port { get; set; } = Constants.DefaultPort;
    public LogLevel LogLevel { get; set; } = Constants.DefaultLogLevel;
}
