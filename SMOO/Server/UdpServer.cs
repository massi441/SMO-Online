using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using SMOO.Protocol;
using SMOO.Memory;

namespace SMOO.Server;

internal class UdpServer
{
    private readonly Channel<Packet> _packets;

    private readonly ServerContext _context;

    public UdpServer(ServerContext context, bool addDefaultRoom = true)
    {
        _context = context;
        _packets = Channel.CreateUnbounded<Packet>();

        if (addDefaultRoom)
        {
            _context.RoomHolder.AddRoom(_context);
        }
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _context.Logger.LogInformation("Server listening on port {Port}...", _context.Config.Port);

        try
        {
            await Task.WhenAll(
                ReceiveLoop(cancellationToken),
                ProcessLoop(cancellationToken)
            );
        }
        catch (OperationCanceledException)
        {
            _context.Logger.LogWarning("Operations canceled.");
        }

        _context.Logger.LogInformation("Shutting down server...");

        await _context.RoomHolder.ShutdownRooms();
    }

    private async Task ReceiveLoop(CancellationToken cancellationTokenSource)
    {
        while (!cancellationTokenSource.IsCancellationRequested)
        {
            using SharedBuffer buffer = new SharedBuffer(Constants.MaxBufferSize);
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            try
            {
                SocketReceiveFromResult receiveResult = await _context.PacketController.ReceiveFromAsync(buffer.Ref, SocketFlags.None, sender, cancellationTokenSource);
                if (receiveResult.ReceivedBytes > 0)
                {
                    buffer.Restrict(receiveResult.ReceivedBytes);
                    _packets.Writer.TryWrite(new Packet
                    {
                        Sender = (IPEndPoint)receiveResult.RemoteEndPoint,
                        Buffer = buffer.Transfer()
                    });
                }
                else
                {
                    _context.Logger.LogInformation("Empty packet received from {Address}:{Port}", sender.Address.ToString(), sender.Port);
                }
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted)
            {
                _context.Logger.LogWarning("Operation aborted");
                break;
            }
            catch (OperationCanceledException)
            {
                _context.Logger.LogWarning("The server was interrupted and will be shutdown");
                break;
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.MessageSize)
            {
                _context.Logger.LogError("The received packet was too big to fit inside the receive buffer");
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
            {
                _context.Logger.LogWarning("The remote host is unreachable, and will shortly be disconnected by the health check");
            }
            catch (Exception ex)
            {
                _context.Logger.LogError(ex, "An Unexpected exception occured while receiving packets");
            }
        }
    }

    private async Task ProcessLoop(CancellationToken cancellationTokenSource)
    {
        await foreach (Packet packet in _packets.Reader.ReadAllAsync(cancellationTokenSource))
        {
            using SharedBuffer buffer = packet.Buffer;

            ServerResult dispatchResult = Dispatch(packet, _context);
            if (dispatchResult.IsFailed)
            {
                _context.Logger.LogWarning("Dispatch failed. Error: {Error}, Sender: {Address}:{Port}", dispatchResult.Error, packet.Sender.Address, packet.Sender.Port);
            }
        }
    }

    private static ServerResult Dispatch(Packet packet, ServerContext context)
    {
        if (!IsValidHeaderSize(packet.Buffer))
        {
            return ServerResult.Failure(ServerError.InvalidHeaderSize);
        }

        ref PacketHeader header = ref packet.Header;

        if (header.Magic != Constants.Magic)
        {
            return ServerResult.Failure(ServerError.InvalidMagic);
        }

        if (!IsValidType((byte)header.Type))
        {
            return ServerResult.Failure(ServerError.InvalidPacketType);
        }

        if (!IsValidVersion(header.Version))
        {
            return ServerResult.Failure(ServerError.InvalidVersion);
        }

        if (header.Type == PacketType.Ping)
        {
            context.Logger.LogTrace("Ping received from {Address}:{Port}", packet.Sender.Address, packet.Sender.Port);
            ServerResult result = context.PacketController.Send(packet.Buffer, packet.Sender);
            return result;
        }

        Room? room = context.RoomHolder.GetRoom(header.RoomId);
        if (room == null)
        {
            return ServerResult.Failure(ServerError.RoomNotFound);
        }

        Packet roomPacket = new Packet
        {
            Buffer = packet.Buffer.Transfer(),
            Sender = packet.Sender
        };

        room.Packets.Writer.TryWrite(roomPacket);

        return ServerResult.Success();
    }

    private static bool IsValidVersion(byte version)
    {
        return version == Constants.Version;
    }

    private static bool IsValidHeaderSize(ReadOnlySpan<byte> span)
    {
        return span.Length >= Unsafe.SizeOf<PacketHeader>();
    }

    private static bool IsValidType(byte packetType)
    {
        return packetType >= 0 && packetType < (byte)PacketType.Invalid;
    }
}
