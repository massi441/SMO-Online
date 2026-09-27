using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Microsoft.Extensions.Logging;
using SMOO.Server;
using SMOO.Services.Impl;

namespace SMOO;

class Program
{
    static async Task Main(string[] args)
    {
        Version? version = Assembly.GetEntryAssembly()?.GetName().Version;

        Console.WriteLine("Yo Miggy: " + version);

        ServerConfig config = Configurator.Load();

        if (ServerLogger.LogLevel != config.LogLevel)
        {
            ServerLogger.Instance().LogInformation("Updating log level from {OldLevel} to {NewLevel}", ServerLogger.LogLevel, config.LogLevel);
            ServerLogger.LogLevel = config.LogLevel;
        }

        try
        {
            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint listenEndpoint = new IPEndPoint(IPAddress.Any, config.Port);

            socket.Bind(listenEndpoint);

            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            ServerContext context = CreateContext(socket, config, cancellationTokenSource.Token);
            UdpServer server = new UdpServer(context);

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
            Logger = ServerLogger.Instance(),
            PacketController = new PacketController(socket),
            PlayerDisconnector = new PlayerDisconnector(),
            RoomHolder = new RoomHolder(),
            Config = config
        };
    }
}
