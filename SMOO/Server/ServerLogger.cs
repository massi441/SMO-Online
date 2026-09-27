using Microsoft.Extensions.Logging;

namespace SMOO.Server;

/// <summary>
/// A decorator around an existing logger, with an mutable log level
/// </summary>
internal class ServerLogger : ILogger
{
    private readonly ILogger _logger;

    public LogLevel LogLevel { get; set; }

    public ServerLogger(ILogger logger, LogLevel level = LogLevel.Information)
    {
        _logger = logger;
        LogLevel = level;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        _logger.Log(logLevel, eventId, state, exception, formatter);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LogLevel && _logger.IsEnabled(logLevel);
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return _logger.BeginScope(state);
    }
}

internal static class ServerLoggerFactory
{
    private static readonly ILoggerFactory _loggerFactory = null!;
    private static readonly ServerLogger _serverLogger = null!;

    static ServerLoggerFactory()
    {
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            });

            builder.SetMinimumLevel(LogLevel.Trace); // that way it always passes the log level check in the decorator
        });

        ILogger logger = _loggerFactory.CreateLogger("Server");
        _serverLogger = new ServerLogger(logger);
    }

    public static ServerLogger Instance()
    {
        return _serverLogger;
    }

}
