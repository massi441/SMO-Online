using System.Runtime.InteropServices;
using SMOO.Client;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Protocol;

internal class ReliablePacket
{
    private byte _tries;
    public byte Tries { get => _tries; init => _tries = value; }
    public required ushort SequenceNumber { get; init; }
    public required Player Receiver { get; init; }
    public required SharedBuffer Buffer { get; init; }
    public required int ResendMsDelay { get; init; }
    public ref PacketHeader Header => ref MemoryMarshal.AsRef<PacketHeader>(Buffer.UsedSpan);
    public DateTime LastSent { get; private set; } = DateTime.UtcNow;
    public bool HasTriesLeft => _tries > 0;

    public void RefreshLastSent()
    {
        LastSent = DateTime.UtcNow;
    }

    public void DecrementTries()
    {
        if (_tries > 0)
        {
            _tries--;
        }
    }

    public bool IsResendTime()
    {
        return !Receiver.IsDisconnected && (DateTime.UtcNow - LastSent).TotalMilliseconds > ResendMsDelay;
    }

    public void WriteSequenceNumber()
    {
        PacketUtil.WriteSequenceNumber(Buffer.UsedSpan, SequenceNumber);
    }
}
