using Microsoft.Extensions.Logging;
using SMOO.Client;
using SMOO.Enumerator;
using SMOO.Memory;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Interface;
using SMOO.Threading;

namespace SMOO.Services.Impl;

internal class ReliablePacketStore : IReliablePacketStore
{
    private readonly ServerContext _context;
    private readonly LockedDictionary<Player, Dictionary<ushort, ReliablePacket>> _pendingPackets;
    private ushort _nextSequenceNumber = 0;

    public LockedDictionary<Player, Dictionary<ushort, ReliablePacket>> PendingPackets => _pendingPackets;

    public ReliablePacketStore(ServerContext context)
    {
        _context = context;
        _pendingPackets = new LockedDictionary<Player, Dictionary<ushort, ReliablePacket>>();
    }

    public ReliablePacket UploadPacket(SharedBuffer buffer, Player receiver, byte maxRetries, int resendDelay)
    {
        using Lock.Scope scope = _pendingPackets.EnterScope();

        ReliablePacket reliablePacket = UploadPlayer(buffer, receiver, maxRetries, resendDelay);

        _nextSequenceNumber++;

        _context.Logger.LogTrace("Uploaded reliable {PacketType} packet with sequence number #{SequenceNumber}, and {Tries} tries", reliablePacket.Header.Type, reliablePacket.SequenceNumber, reliablePacket.Tries);

        return reliablePacket;
    }

    public void UploadBroadcast<TEnumerator>(SharedBuffer buffer, TEnumerator players, byte maxRetries, int resendDelay) where TEnumerator : IPlayerEnumerator<TEnumerator>, allows ref struct
    {
        using Lock.Scope scope = _pendingPackets.EnterScope();

        foreach (Player player in players)
        {
            ReliablePacket reliablePacket = UploadPlayer(buffer, player, maxRetries, resendDelay);
            _context.Logger.LogTrace("Uploaded broadcastable reliable {PacketType} packet with sequence number #{SequenceNumber}, and {Tries} tries", reliablePacket.Header.Type, reliablePacket.SequenceNumber, reliablePacket.Tries);
        }

        _nextSequenceNumber++; // Potential TODO: fix case where no players is in the enumerator
    }

    public ReliablePacket? RemovePacket(Player requester, ushort sequenceNumber)
    {
        using Lock.Scope scope = _pendingPackets.EnterScope();

        if (_pendingPackets.TryGetValue(requester, out Dictionary<ushort, ReliablePacket>? playerPackets)) 
        {
            if (!playerPackets.Remove(sequenceNumber, out ReliablePacket? pendingPacket))
            {
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

        _context.Logger.LogWarning("Reliable packet #{SequenceNumber} not found for {PlayerName}", sequenceNumber, requester.Name);

        return null;
    }

    public void ClearPlayer(Player player)
    {
        using Lock.Scope scope = _pendingPackets.EnterScope();

        if (_pendingPackets.TryGetValue(player, out Dictionary<ushort, ReliablePacket>? playerDict))
        {
            foreach (var entry in playerDict)
            {
                entry.Value.Buffer.Release();
            }

            _pendingPackets.Remove(player);

            _context.Logger.LogInformation("Successfully cleared {PlayerName}'s reliable packets", player.Name);
        }
    }

    // note: lock free, make sure to always call with a lock
    private ReliablePacket UploadPlayer(SharedBuffer buffer, Player receiver, byte maxRetries, int resendDelay)
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

        if (!_pendingPackets.ContainsKey(receiver))
        {
            _pendingPackets[receiver] = [];
        }

        _pendingPackets[receiver][_nextSequenceNumber] = reliablePacket;

        return reliablePacket;
    }
}
