using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using SMOO.Protocol;
using SMOO.Services.Impl;
using SMOO.Util;

namespace SMOO.Server;

internal class UdpServer
{
    private readonly int _port;
    private readonly Channel<Packet> _packets;

    private ServerContext _context = null!;

    public UdpServer(int port)
    {
        _port = port;
        _packets = Channel.CreateUnbounded<Packet>();
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        IPEndPoint listenEndpoint = new IPEndPoint(IPAddress.Any, _port);

        socket.Bind(listenEndpoint);

        InitContext(socket, cancellationToken);

        _context.Logger.LogInformation("Server listening on port {Port}...", _port);

        try
        {
            await Task.WhenAll(
                ReceiveLoop(socket, cancellationToken),
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

    private async Task ReceiveLoop(Socket socket, CancellationToken cancellationTokenSource)
    {
        while (!cancellationTokenSource.IsCancellationRequested)
        {
            using SharedBuffer buffer = new SharedBuffer(Config.MaxBufferSize);
            try
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

                SocketReceiveFromResult receiveResult = await socket.ReceiveFromAsync(buffer.Ref, SocketFlags.None, sender, cancellationTokenSource);
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
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.MessageSize)
            {
                _context.Logger.LogError("The received packet was too big to fit inside the receive buffer");
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

            Result<Error> dispatchResult = Dispatch(packet, _context);
            if (dispatchResult.IsFailed)
            {
                _context.Logger.LogWarning("Dispatch failed. Error: {Error}, Sender: {Address}:{Port}", dispatchResult.Error, packet.Sender.Address, packet.Sender.Port);
            }
        }
    }

    private static Result<Error> Dispatch(Packet packet, ServerContext context)
    {
        if (!IsValidHeaderSize(packet.Buffer))
        {
            return Result<Error>.Failure(Error.InvalidHeaderSize);
        }

        ref PacketHeader header = ref packet.Header;

        if (header.Magic != Config.Magic)
        {
            return Result<Error>.Failure(Error.InvalidMagic);
        }

        if (!IsValidType((byte)header.Type))
        {
            return Result<Error>.Failure(Error.InvalidPacketType);
        }

        if (!IsValidVersion(header.Version))
        {
            return Result<Error>.Failure(Error.InvalidVersion);
        }

        if (header.Type == PacketType.Ping)
        {
            context.Logger.LogTrace("Ping received from {Address}:{Port}", packet.Sender.Address, packet.Sender.Port);
            Result<Error> result = context.PacketSender.Send(packet.Sender, packet.Buffer);
            return result;
        }

        Room? room = context.RoomHolder.GetRoom(header.RoomId);
        if (room == null)
        {
            return Result<Error>.Failure(Error.RoomNotFound);
        }

        Packet roomPacket = new Packet
        {
            Buffer = packet.Buffer.Transfer(),
            Sender = packet.Sender
        };

        room.Packets.Writer.TryWrite(roomPacket);

        return Result<Error>.Success();
    }

    private static bool IsValidVersion(byte version)
    {
        return version == Config.Version;
    }

    private static bool IsValidHeaderSize(ReadOnlySpan<byte> span)
    {
        return span.Length >= Unsafe.SizeOf<PacketHeader>();
    }

    private static bool IsValidType(byte packetType)
    {
        return packetType >= 0 && packetType < (byte)PacketType.Invalid;
    }

    private void InitContext(Socket socket, CancellationToken cancellationToken)
    {
        _context = new ServerContext()
        {
            Logger = LockstepLogger.Instance(),
            RoomHolder = new RoomHolder(),
            PacketSender = new PacketSender(socket),
            PlayerDisconnector = new PlayerDisconnector(),
            CancellationToken = cancellationToken
        };

        _context.RoomHolder.AddRoom(_context);
    }
}
