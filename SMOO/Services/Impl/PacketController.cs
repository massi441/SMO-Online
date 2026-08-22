using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Interface;
using SMOO.Memory;

namespace SMOO.Services.Impl;


internal class PacketController : IPacketController
{
    private readonly Socket _socket;

    public PacketController(Socket socket)
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
        return Send(buffer, receiver.Endpoint);
    }

    public ServerResult SendAck(ParsedPacket originalPacket)
    {
        PacketAck ackPacket = new PacketAck()
        {
            Header = originalPacket.Header.WithType(PacketType.Ack),
        };

        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<PacketAck>()];

        SpanWriter writer = new SpanWriter(buffer);

        ackPacket.Serialize(ref writer);

        return Send(buffer, originalPacket.SenderIp);
    }

    public void SendReliably(SharedBuffer buffer, Player receiver, Room room, byte maxRetries, int resendDelay)
    {
        room.Broadcaster.ReliablePacketStore.UploadPacket(buffer, receiver, maxRetries, resendDelay);

        Send(buffer.UsedSpan, receiver);
    }

    public ValueTask<SocketReceiveFromResult> ReceiveFromAsync(Memory<byte> buffer, SocketFlags flags, EndPoint remoteEndPoint, CancellationToken cancellationToken = default)
    {
        return _socket.ReceiveFromAsync(buffer, flags, remoteEndPoint, cancellationToken);
    }
}
