using System.Net;
using System.Net.Sockets;
using Core.Github;
using Microsoft.Extensions.Logging;
using SMOO.Server;
using SMOO.Services.Impl;
using SMOO.Updater.Lib;

namespace SMOO;

class Program
{
    static async Task Main(string[] args)
    {
        SMOOUpdater.WipeUpdateTempDirFrom();

        SMOOUpdater updater = new SMOOUpdater();

        ServerLogger logger = ServerLoggerFactory.Instance();

        try
        {
            GithubUpdateCheck updateCheck = await updater.CheckUpdate();
            if (updateCheck.IsNeedUpdate())
            {
                Console.WriteLine($"You are currently on version {updateCheck.CurrentVersion} of SMOO, but version {updateCheck.LatestVersion} is available.");
                Console.Write("Would you like to download it? (y): ");

                string? input = Console.ReadLine();

                if (input == "y" && SMOOUpdater.StartUpdater())
                {
                    Console.WriteLine("Launching updater");
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

            SMOOServer server = new SMOOServer(context);

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
            RoomHolder = new RoomHolder(),
            Config = config
        };
    }
}
