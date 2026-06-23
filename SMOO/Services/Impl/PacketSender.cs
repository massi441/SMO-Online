using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Interface;
using SMOO.Memory;

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

    public void SendReliably(SharedBuffer buffer, Player receiver, IReliablePacketStore reliableStore, byte maxRetries, int resendDelay)
    {
        reliableStore.UploadPacket(buffer, receiver, maxRetries, resendDelay);

        Send(buffer.UsedSpan, receiver);
    }
}
