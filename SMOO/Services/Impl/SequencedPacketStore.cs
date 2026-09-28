using Core.Memory;
using Microsoft.Extensions.Logging;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Interface;

namespace SMOO.Services.Impl;

internal class SequencedPacketStore : ISequencedPacketStore
{
    private readonly ServerContext _context;
    private readonly SequencedPacket?[] _packets;
    private ushort _nextSequenceNumber = 0;

    internal const ushort MaxStoreSize = 32;

    public int Size => _packets.Length;

    public SequencedPacketStore(ServerContext context)
    {
        _context = context;
        _packets = new SequencedPacket[MaxStoreSize];
    }

    public ServerResult<SequencedPacket> UploadPacket(Player receiver, RentedBuffer buffer, SequencedPacketParams packetParams = default)
    {
        ushort slot = CalcSlot(_nextSequenceNumber);

        SequencedPacket? existingPacket = _packets[slot];

        if (existingPacket != null)
        {
            _context.Logger.LogWarning("Tried to insert packet #{SequenceNumber} in slot {Slot} for {PlayerName}, but the store was full", _nextSequenceNumber, slot, receiver.Name);
            return ServerResult<SequencedPacket>.Failure(ServerError.PendingPacketStoreFull);
        }

        SequencedPacket newPacket = new SequencedPacket(packetParams)
        {
            Buffer = buffer,
            Receiver = receiver,
            SequenceNumber = _nextSequenceNumber,
        };

        buffer.Acquire();
        newPacket.WriteSequenceNumber();

        _packets[slot] = newPacket;

        _nextSequenceNumber++;

        return ServerResult<SequencedPacket>.Success(newPacket);
    }

    public SequencedPacket? RemovePacket(ushort sequenceNumber)
    {
        ushort slot = CalcSlot(sequenceNumber);

        SequencedPacket? packet = _packets[slot];
        if (packet == null)
        {
            return null;
        }

        if (packet.SequenceNumber != sequenceNumber)
        {
            return null;
        }

        return ClearPacketIfPresent(slot);
    }

    public void Clear()
    {
        for (int i = 0; i < _packets.Length; i++)
        {
            SequencedPacket? releasedPacket = ClearPacketIfPresent(i);
            if (releasedPacket != null)
            {
                _context.Logger.LogInformation("Cleared sequenced {SequenceNumber} packet from {PlayerName}", releasedPacket.SequenceNumber, releasedPacket.Receiver.Name);
            }
        }
    }

    public void ResendPackets()
    {
        for (int i = 0; i < _packets.Length; i++)
        {
            SequencedPacket? packet = _packets[i];
            if (packet == null)
            {
                continue;
            }

            if (packet.HasTriesLeft())
            {
                TryResendPacket(packet);
            }
            else
            {
                _context.Logger.LogInformation("{Player} in room #{RoomId} failed to ack packet #{SequenceNumber} and will be disconnected", packet.Receiver.Name, packet.Receiver.Room.Id, packet.SequenceNumber);
                packet.Receiver.Room.RequestDisconnection(packet.Receiver);
                return;
            }
        }
    }

    private void TryResendPacket(SequencedPacket packet)
    {
        if (!packet.IsResendTime())
        {
            return;
        }

        _context.Logger.LogTrace("Resending {Type} packet #{Id} to {PlayerName} in room {#RoomdId}", packet.Header.Type, packet.SequenceNumber, packet.Receiver.Name, packet.Receiver.Room.Id);

        packet.WriteSequenceNumber();

        ServerResult sendResult = _context.PacketController.Send(packet.Buffer, packet.Receiver);
        if (!sendResult.IsSuccess)
        {
            _context.Logger.LogError("An error occured while trying to resend the packet: {Error}", sendResult.Error);
        }

        packet.DecrementTries();
        packet.RefreshLastSent();
    }

    private ushort CalcSlot(ushort sequenceNumber)
    {
        return (ushort)(sequenceNumber % _packets.Length);
    }

    private SequencedPacket? ClearPacketIfPresent(int slot)
    {
        SequencedPacket? packet = _packets[slot];
        if (packet == null)
        {
            return null;
        }

        packet.Buffer.Release();

        _packets[slot] = null;

        return packet;
    }
}
