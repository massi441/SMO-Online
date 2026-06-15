using System.Net;
using System.Net.Sockets;
using SMOO.Client;
using SMOO.Server;
using SMOO.Services.Interface;
using SMOO.Util;

namespace SMOO.Services.Impl;

internal class PacketSender : IPacketSender
{
    private readonly Socket _socket;

    public PacketSender(Socket socket)
    {
        _socket = socket;
    }

    public ServerResult Send(ReadOnlySpan<byte> buffer, IPEndPoint receiver)
    {
        int bytesSent = _socket.SendTo(buffer, receiver);
        if (bytesSent != buffer.Length)
        {
            return ServerResult.Failure(ServerError.NotSent);
        }

        return ServerResult.Success();
    }

    public ServerResult Send(ReadOnlySpan<byte> buffer, Player receiver)
    {
        try
        {
            int bytesSent = _socket.SendTo(buffer, receiver.Endpoint);
            if (bytesSent != buffer.Length)
            {
                return ServerResult.Failure(ServerError.NotSent);
            }

            return ServerResult.Success();
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
        {
            receiver.Room.DropPlayer(receiver);
            return ServerResult.Failure(ServerError.ConnectionLost);
        }
    }

    public void SendReliably(SharedBuffer buffer, Player receiver, IReliablePacketStore reliableStore, byte maxRetries = Config.MaxRetries)
    {
        reliableStore.UploadPacket(buffer, receiver, maxRetries);

        Send(buffer.UsedSpan, receiver);
    }
}
