using System.Collections.Concurrent;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using SMOO.Client;
using SMOO.Handle;
using SMOO.Protocol;
using SMOO.Services.Interface;
using SMOO.Util;

namespace SMOO.Server;

internal class Room
{
    private readonly ServerContext _context;
    private readonly Task _processTask;
    private readonly Task _healthCheckTask;
    private readonly ConcurrentQueue<Action> _commands = [];
    private readonly CancellationTokenSource _healthCheckToken;

    public ushort Id { get; }
    public Channel<Packet> Packets { get; }
    public IPlayerHolder PlayerHolder { get; }
    public IBroadcaster Broadcaster { get; }
    public PlayerList Players => PlayerHolder.Players;
    public IReliablePacketStore ReliableStore => Broadcaster.ReliablePacketStore;

    public Room(ushort roomId, ServerContext conxtext, IPlayerHolder playerHolder, IBroadcaster broadcaster)
    {
        _context = conxtext;

        Id = roomId;
        PlayerHolder = playerHolder;
        Packets = Channel.CreateUnbounded<Packet>();
        Broadcaster = broadcaster;

        _healthCheckToken = CancellationTokenSource.CreateLinkedTokenSource(_context.CancellationToken);

        _processTask = Task.Run(ProcessAsync, _context.CancellationToken);
        _healthCheckTask = Task.Run(CheckIdlePlayers, _context.CancellationToken);
    }

    public Task Shutdown()
    {
        Packets.Writer.Complete();
        _healthCheckToken.Cancel();
        _healthCheckToken.Dispose();
        return Task.WhenAll(_processTask, _healthCheckTask, Broadcaster.Shutdown());
    }

    public void UploadCommand(Action action)
    {
        _commands.Enqueue(action);
    }

    public void DisconnectPlayer(Player player)
    {
        player.MarkDisconnected();

        UploadCommand(() =>
        {
            ServerResult disconnectResult = _context.PlayerDisconnector.Disconnect(player);
            if (disconnectResult.IsSuccess)
            {
                _context.Logger.LogInformation("Successfully disconnected {PlayerName} from Room #{RoomId}", player.Name, player.Room.Id);
            }
            else
            {
                _context.Logger.LogError("Failed to disconnect player {PlayerName} in Room #{RoomId}: {Error}", player.Name, Id, disconnectResult.Error!.Value);
            }
        });
    }

    private async Task ProcessAsync()
    {
        await foreach (Packet packet in Packets.Reader.ReadAllAsync())
        {
            try
            {
                using SharedBuffer buffer = packet.Buffer;

                while (_commands.TryDequeue(out Action? command))
                {
                    _context.Logger.LogTrace("Processing command in room #{RoomId}", Id);
                    command!.Invoke();
                }

                if (!IsAllowedInRoom(packet.Sender, packet.Header, out Player? player))
                {
                    _context.Logger.LogWarning("{Address}:{Port} illegally tried to access room #{RoomId}", packet.Sender.Address, packet.Sender.Port, Id);
                    continue;
                }

                player?.RefreshLastSeen();

                PacketHandler packetHandler = PacketHandlerTable.GetHandler(packet.Header.Type);

                if (packet.PayloadSize < packetHandler.MinPayloadSize)
                {
                    _context.Logger.LogWarning("{PacketType} packet of invalid size ({PacketSize}) was requested. Minimum required: {Minimum}", packet.Header.Type, packet.PayloadSize, packetHandler.MinPayloadSize);
                    continue;
                }

                if (packet.PayloadSize > packetHandler.MaxPayloadSize)
                {
                    _context.Logger.LogWarning("{PacketType} packet payload too large ({PacketSize}), maximum allowed: {Maximum}. Error: {Error}", packet.Header.Type, packet.PayloadSize, packetHandler.MaxPayloadSize, ServerError.PayloadTooLarge);
                    continue;
                }

                ParsedPacket parsedPacket = new ParsedPacket()
                {
                    SenderPlayer = player,
                    Buffer = packet.Buffer,
                    SenderIp = packet.Sender
                };

                unsafe
                {
                    packetHandler.Handler(parsedPacket, this, _context);
                }
            }
            catch (InvalidDataException ex)
            {
                _context.Logger.LogError("Invalid data detected in {PacketType} in Room #{RoomId}: {Message}", packet.Header.Type, Id, ex.Message);
            }
            catch (Exception ex)
            {
                _context.Logger.LogError(ex, "Unexpected error in Room #{RoomId}", Id);
            }
        }

        _context.Logger.LogInformation("Room #{RoomId} was shutdown sucessfully", Id);
    }

    private async Task CheckIdlePlayers()
    {
        while (!_healthCheckToken.IsCancellationRequested)
        {
            foreach (Player player in PlayerHolder.Players)
            {
                if (player.IsDisconnected)
                {
                    continue;
                }

                if (player.IsConnectionLost())
                {
                    DisconnectPlayer(player);
                    _context.Logger.LogWarning("Player {PlayerName} has lost connection in Room #{RoomId} and will be disconnected", player.Name, Id);
                    continue;
                }
                
                if (player.IsNeedHealthCheck())
                {
                    PacketHeader header = new PacketHeader()
                    {
                        Type = PacketType.HealthCheck,
                        Flags = 0,
                        Version = Config.Version,
                        RoomId = Id
                    };

                    using SharedBuffer buffer = PacketSerializer.Serialize(ref header, Unsafe.SizeOf<PacketHeader>());

                    _context.Logger.LogTrace("Player {PlayerName} has been idle for too long in Room #{RoomId}, a health check request will be sent", player.Name, Id);

                    _context.PacketSender.Send(buffer, player);
                }
            }

            try
            {
                await Task.Delay(Config.PlayerHealthCheckTick, _healthCheckToken.Token);
            }
            catch (OperationCanceledException)
            {
                _context.Logger.LogTrace("Health check delay was cancelled, Room is shutting down...");
            }
        }
    }

    private bool IsAllowedInRoom(IPEndPoint sender, PacketHeader header, out Player? player)
    {
        if (header.Type == PacketType.ConnectSyn)
        {
            player = null;
            return true;
        }

        player = PlayerHolder.FindPlayerByHost(sender);

        return player != null;
    }
}
