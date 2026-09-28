using System.Net;
using System.Net.Sockets;
using SMOO.Server;
using SMOO.Services.Impl;

namespace SMOO;

/// <summary>
/// Builds a SMOO server with its dependencies
/// </summary>
internal static class ServerBuilder
{
    /// <summary>
    /// Builds a server listening on the configured port.
    /// </summary>
    public static SMOOServer Build(ServerConfig config, CancellationToken cancellationToken)
    {
        Socket socket = CreateSocket(config);
        ServerContext context = CreateContext(socket, cancellationToken);

        return new SMOOServer(context, socket);
    }

    private static Socket CreateSocket(ServerConfig config)
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        try
        {
            IPEndPoint listenEndpoint = new IPEndPoint(IPAddress.Any, config.Port);

            socket.Bind(listenEndpoint);
        }
        catch
        {
            socket.Dispose();
            throw;
        }

        return socket;
    }

    private static ServerContext CreateContext(Socket socket, CancellationToken cancellationToken)
    {
        return new ServerContext()
        {
            CancellationToken = cancellationToken,
            Logger = ServerLoggerFactory.Instance(),
            PacketController = new PacketController(socket),
            RoomHolder = new RoomHolder(),
        };
    }
}
