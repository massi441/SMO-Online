using SMOO.Server;
using Microsoft.Extensions.Logging;

namespace SMOO;

class Program
{
    static async Task Main(string[] args)
    {
        int port = 5001;

        ILogger logger = ServerLogger.Instance();

        try
        {
            UdpServer server = new UdpServer(port);
            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    cancellationTokenSource.Cancel();
                }
            };

            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    cancellationTokenSource.Cancel();
                }
            };

            await server.RunAsync(cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while the server was running");
        }
    }
}
