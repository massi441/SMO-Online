using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using SMOO.Server;
using SMOO.Services.Impl;
using SMOO.Updater;

namespace SMOO;

class Program
{
    static async Task Main(string[] args)
    {
        ServerLogger logger = ServerLoggerFactory.Instance();

        try
        {
            UpdateCheck updateCheck = await UpdateManager.CheckUpdate();
            if (updateCheck.IsNeedUpdate())
            {
                Console.WriteLine($"You are currently on version {updateCheck.CurrentVersion} of SMOO, but version {updateCheck.LatestVersion} is available.");
                Console.Write("Would you like to download it? (y): ");

                string? input = Console.ReadLine();

                if (input == "y" && StartUpdater())
                {
                    return;
                }
            }

            logger.LogInformation("Current SMOO version ({CurrentVersion}) is up to date", updateCheck.CurrentVersion);
        }
        catch (Exception ex)
        {
            logger.LogWarning("An error occured while looking for an update: {Message}", ex.Message);
        }

        ServerConfig config = Configurator.Load();

        try
        {
            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint listenEndpoint = new IPEndPoint(IPAddress.Any, config.Port);

            socket.Bind(listenEndpoint);

            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            ServerContext context = CreateContext(socket, config, cancellationTokenSource.Token);

            GameServer server = new GameServer(context);

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

            await server.Start(cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred while the server was running: {ex.Message}");
        }
    }

    private static ServerContext CreateContext(Socket socket, ServerConfig config, CancellationToken cancellationToken)
    {
        return new ServerContext()
        {
            CancellationToken = cancellationToken,
            Logger = ServerLoggerFactory.Instance(),
            PacketController = new PacketController(socket),
            PlayerDisconnector = new PlayerDisconnector(),
            RoomHolder = new RoomHolder(),
            Config = config
        };
    }

    private static bool StartUpdater()
    {
        string updaterPath = Path.Combine(AppContext.BaseDirectory, "Updater.exe");

        ProcessStartInfo startupInfo = new ProcessStartInfo(updaterPath)
        {
            UseShellExecute = true
        };

        return Process.Start(startupInfo) != null;
    }
}
