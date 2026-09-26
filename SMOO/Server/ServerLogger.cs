using Microsoft.Extensions.Logging;

namespace SMOO.Server;

internal static class ServerLogger
{
    private static readonly ILoggerFactory _loggerFactory = null!;
    private static readonly ILogger _logger = null!;

    public static LogLevel LogLevel { get; set; } = LogLevel.Information; // default to Information so config loading is visible

    static ServerLogger()
    {
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            });

            builder.AddFilter((miggy, level) =>
            {
                return level >= LogLevel;
            });
        });

        _logger = _loggerFactory.CreateLogger("Server");
    }

    public static ILogger Instance()
    {
        return _logger;
    }
}
