using System.Net;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using SMOO.Client;
using SMOO.Handle;
using SMOO.Protocol;
using SMOO.Services.Interface;
using SMOO.Memory;

namespace SMOO.Server;

internal class Room
{
    private readonly ServerContext _context;
    private readonly Task _processTask;
    private readonly IPlayerHealthChecker _healthChecker;
    
    public ushort Id { get; }
    public IPlayerHolder PlayerHolder { get; }
    public IBroadcaster Broadcaster { get; }
    public IReliablePacketStore ReliableStore => Broadcaster.ReliablePacketStore;
    public Channel<Packet> Packets { get; }
    public PlayerList Players => PlayerHolder.Players;

    public Room(ushort roomId, ServerContext conxtext, IPlayerHolder playerHolder, IBroadcaster broadcaster, IPlayerHealthChecker healthChecker)
    {
        _context = conxtext;
        _healthChecker = healthChecker;

        Id = roomId;
        PlayerHolder = playerHolder;
        Packets = Channel.CreateUnbounded<Packet>();
        Broadcaster = broadcaster;

        _processTask = Task.Run(ProcessAsync, _context.CancellationToken);
    }

    public Task Shutdown()
    {
        Packets.Writer.Complete();
        return Task.WhenAll(_processTask, _healthChecker.Shutdown(), Broadcaster.Shutdown());
    }

    private async Task ProcessAsync()
    {
        await foreach (Packet packet in Packets.Reader.ReadAllAsync())
        {
            try
            {
                using SharedBuffer buffer = packet.Buffer;

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
