using Microsoft.Extensions.Logging;

namespace SMOO.Server;

internal static class ServerLogger
{
    private static ILogger _logger = null!;
    private static ILoggerFactory _loggerFactory = null!;
    private static readonly Lock _lock = new Lock();

    public static ILogger Instance(LogLevel logLevel)
    {
        if (_logger == null)
        {
            lock (_lock)
            {
                if (_logger == null)
                {
                    _loggerFactory = LoggerFactory.Create(builder =>
                    {
                        builder.AddSimpleConsole(options =>
                        {
                            options.SingleLine = true;
                            options.TimestampFormat = "HH:mm:ss ";
                        });

                        builder.SetMinimumLevel(logLevel);
                    });

                    _logger = _loggerFactory.CreateLogger("Server");
                }
            }
        }

        return _logger;
    }
}
