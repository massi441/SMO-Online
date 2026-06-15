using System.Net;
using System.Net.Sockets;
using SMOO.Client;
using SMOO.Protocol;
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

    public Result<Error> Send(EndPoint destination, ReadOnlySpan<byte> buffer)
    {
        int bytesSent = _socket.SendTo(buffer, destination);
        if (bytesSent != buffer.Length)
        {
            return Result<Error>.Failure(Error.NotSent);
        }

        return Result<Error>.Success();
    }

    public void SendReliably(Player receiver, SharedBuffer buffer, Room room, byte maxRetries = Config.MaxRetries)
    {
        room.Broadcaster.ReliablePacketStore.UploadPacket(buffer, receiver, maxRetries);

        Send(receiver.Endpoint, buffer.UsedSpan);
    }
}
