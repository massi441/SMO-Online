using Microsoft.Extensions.Logging;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Interface;

namespace SMOO.Services.Impl;

/// <summary>
/// Resends reliable packets if they have not been acknowledged after a certain delay
/// </summary>
internal class PacketResendMessageProcessor : IRoomMessageProcessor
{
    private readonly ServerContext _context;
    private readonly IReliablePacketStore _reliablePacketStore;
    private readonly Stack<ReliablePacket> _deadPackets;

    public PacketResendMessageProcessor(ServerContext context, IReliablePacketStore reliableStore)
    {
        _context = context;
        _reliablePacketStore = reliableStore;
        _deadPackets = [];
    }

    public void Process(Room room, Packet packet)
    {
        foreach (var playerPackets in _reliablePacketStore.PendingPackets)
        {
            foreach (var playerPacket in playerPackets.Value)
            {
                ReliablePacket reliablePacket = playerPacket.Value;
                if (reliablePacket.HasTriesLeft)
                {
                    TryResendPacket(reliablePacket);
                }
                else
                {
                    _deadPackets.Push(reliablePacket);
                }
            }
        }

        while (_deadPackets.TryPop(out ReliablePacket? deadPacket))
        {
            TryClearPacket(deadPacket);
        }
    }

    private void TryResendPacket(ReliablePacket packet)
    {
        if (!packet.IsResendTime())
        {
            return;
        }

        _context.Logger.LogTrace("Resending {Type} packet #{Id} to {PlayerName} in room {#RoomdId}", packet.Header.Type, packet.SequenceNumber, packet.Receiver.Name, packet.Receiver.Room.Id);

        try
        {
            ServerResult sendResult = _context.PacketController.Send(packet.Buffer, packet.Receiver);
            if (!sendResult.IsSuccess)
            {
                _context.Logger.LogError("An error occured while trying to resend the packet: {Error}", sendResult.Error);
            }
        }
        catch (Exception ex)
        {
            _context.Logger.LogError("Failed to resend packet: {Message}", ex.Message);
        }

        packet.DecrementTries();
        packet.RefreshLastSent();
    }

    private void TryClearPacket(ReliablePacket reliablePacket)
    {
        ReliablePacket? expiredPacket = _reliablePacketStore.RemovePacket(reliablePacket.Receiver, reliablePacket.SequenceNumber);
        if (expiredPacket == null)
        {
            _context.Logger.LogWarning("Expired packet already removed");
            return;
        }

        ServerResult disconnectResult = _context.PlayerDisconnector.Disconnect(reliablePacket.Receiver);
        if (disconnectResult.IsSuccess)
        {
            _context.Logger.LogWarning("Disconnected player {PlayerName} for not acking packet (#{SequenceNumber}) in room #{RoomId}", reliablePacket.Receiver.Name, reliablePacket.SequenceNumber, reliablePacket.Receiver.Room.Id);
        }
        else
        {
            _context.Logger.LogError("Failed to disconnect player {PlayerName} for not Acking packet #{PacketId} in room #{RoomId}", reliablePacket.Receiver.Name, reliablePacket.SequenceNumber, reliablePacket.Receiver.Room.Id);
        }
    }
}
