using Microsoft.Extensions.Logging;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Interface;
using SMOO.Util;

namespace SMOO.Services.Impl;

internal class ReliablePacketStore : IReliablePacketStore
{
    private readonly ServerContext _context;
    private readonly LockedDictionary<ushort, ReliablePacket> _pendingPackets;
    private ushort _nextSequenceNumber = 0;

    public LockedDictionary<ushort, ReliablePacket> PendingPackets => _pendingPackets;


    public ReliablePacketStore(ServerContext context)
    {
        _context = context;
        _pendingPackets = new LockedDictionary<ushort, ReliablePacket>();
    }

    public ReliablePacket UploadPacket(SharedBuffer buffer, Player receiver, byte maxRetries, int resendDelay)
    {
        ReliablePacket reliablePacket = new ReliablePacket()
        {
            Buffer = buffer,
            Receiver = receiver,
            Tries = maxRetries,
            SequenceNumber = _nextSequenceNumber,
            ResendMsDelay = resendDelay
        };

        reliablePacket.Buffer.Acquire();
        reliablePacket.WriteSequenceNumber();
        
        _pendingPackets[_nextSequenceNumber] = reliablePacket;

        _nextSequenceNumber++;

        _context.Logger.LogTrace("Uploaded reliable {PacketType} packet with sequence number #{SequenceNumber}, and {Tries} tries", reliablePacket.Header.Type, reliablePacket.SequenceNumber, reliablePacket.Tries);

        return reliablePacket;
    }

    public ReliablePacket? RemovePacket(Player requester, ushort sequenceNumber)
    {
        if (_pendingPackets.Remove(sequenceNumber, out ReliablePacket? pendingPacket))
        {
            if (pendingPacket.Receiver != requester)
            {
                _pendingPackets[sequenceNumber] = pendingPacket;
                _context.Logger.LogCritical("Attack detected: {RequesterName} tried to ack {ReceiverName}'s packet (#{SequenceNumber}) in Room #{RoomId}", requester.Name, pendingPacket.Receiver.Name, sequenceNumber, requester.Room.Id);
                return null;
            }

            if (pendingPacket.Buffer.Release())
            {
                _context.Logger.LogTrace("Removed and free'd buffer used by reliable packet #{SequenceNumber}", sequenceNumber);
            }
            else
            {
                _context.Logger.LogTrace("Decremented ref count after removing reliable packet #{SequenceNumber}, new ref count: {RefCount}", sequenceNumber, pendingPacket.Buffer.RefCount);
            }
            
            return pendingPacket;
        }

        return null;
    }

    public void ClearPlayer(Player player)
    {
        _pendingPackets.Lock();

        ReliablePacket[] playerPackets = [.. _pendingPackets.Values.Where(packet => packet.Receiver == player)];

        _pendingPackets.Unlock();

        foreach (ReliablePacket playerPacket in playerPackets)
        {
            RemovePacket(playerPacket.Receiver, playerPacket.SequenceNumber);
        }
    }
}
