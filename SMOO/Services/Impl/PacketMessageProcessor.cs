using System.Net;
using Microsoft.Extensions.Logging;
using SMOO.Client;
using SMOO.Handle;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Interface;

namespace SMOO.Services.Impl;

/// <summary>
/// Processes an incoming Room packet and dispatches it to the right packet handler
/// </summary>
internal class PacketMessageProcessor : IRoomMessageProcessor
{
    private readonly ServerContext _context;

    public PacketMessageProcessor(ServerContext context)
    {
        _context = context;
    }

    public void Process(Room room, Packet packet)
    {
        try
        {
            if (!IsAllowedInRoom(room, packet.Sender, packet.Header, out Player? player))
            {
                _context.Logger.LogWarning("{Address}:{Port} illegally tried to access room #{RoomId}", packet.Sender.Address, packet.Sender.Port, room.Id);
                return;
            }

            player?.RefreshLastSeen();

            PacketHandler packetHandler = PacketHandlerTable.GetHandler(packet.Header.Type);

            if (packet.PayloadSize < packetHandler.MinPayloadSize)
            {
                _context.Logger.LogWarning("{PacketType} packet of invalid size ({PacketSize}) was requested. Minimum required: {Minimum}", packet.Header.Type, packet.PayloadSize, packetHandler.MinPayloadSize);
                return;
            }

            if (packet.PayloadSize > packetHandler.MaxPayloadSize)
            {
                _context.Logger.LogWarning("{PacketType} packet payload too large ({PacketSize}), maximum allowed: {Maximum}. Error: {Error}", packet.Header.Type, packet.PayloadSize, packetHandler.MaxPayloadSize, ServerError.PayloadTooLarge);
                return;
            }

            ParsedPacket parsedPacket = new ParsedPacket()
            {
                SenderPlayer = player,
                Buffer = packet.Buffer,
                SenderIp = packet.Sender
            };

            unsafe
            {
                packetHandler.Handler(parsedPacket, room, _context);
            }
        }
        catch (InvalidDataException ex)
        {
            _context.Logger.LogError("Invalid data detected in {PacketType} in Room #{RoomId}: {Message}", packet.Header.Type, room.Id, ex.Message);
        }
        catch (Exception ex)
        {
            _context.Logger.LogError(ex, "Unexpected error in Room #{RoomId}", room.Id);
        }
    }

    private static bool IsAllowedInRoom(Room room, IPEndPoint sender, PacketHeader header, out Player? player)
    {
        if (header.Type == PacketType.ConnectSyn)
        {
            player = null;
            return true;
        }

        player = room.PlayerHolder.FindPlayerByHost(sender);

        return player != null;
    }
}
