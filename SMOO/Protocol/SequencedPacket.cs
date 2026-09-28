using System.Runtime.InteropServices;
using SMOO.Client;
using SMOO.Server;
using Core.Memory;

namespace SMOO.Protocol;

internal class SequencedPacket
{
    private int _tries;
    public int Tries { get => _tries; private set => _tries = value; }
    public int ResendMsDelay { get; }
    public required ushort SequenceNumber { get; init; }
    public required Player Receiver { get; init; }
    public required RentedBuffer Buffer { get; init; }
    public ref PacketHeader Header => ref MemoryMarshal.AsRef<PacketHeader>(Buffer.UsedSpan);
    public DateTime LastSent { get; private set; } = DateTime.UtcNow;

    public SequencedPacket(SequencedPacketParams packetParams)
    {
        Tries = packetParams.Tries;
        ResendMsDelay = packetParams.ResendMsDelay;
    }

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
        return (DateTime.UtcNow - LastSent).TotalMilliseconds > ResendMsDelay;
    }

    public bool HasTriesLeft()
    { 
        return _tries > 0; 
    }

    public void WriteSequenceNumber()
    {
        PacketUtil.WriteSequenceNumber(Buffer.UsedSpan, SequenceNumber);
    }
}

internal readonly struct SequencedPacketParams
{
    internal const int DefaultTries = 3;
    internal const int DefaultMsDelay = 500;

    private readonly int _tries;
    private readonly int _resendMsDelay;

    public int Tries
    {
        get
        {
            if (_tries > 0)
            {
                return _tries;
            }

            return DefaultTries;
        }
        init
        {
            _tries = value;
        }
    }

    public int ResendMsDelay
    {
        get
        {
            if (_resendMsDelay > 0)
            {
                return _resendMsDelay;
            }

            return DefaultMsDelay;
        }
        init
        {
            _resendMsDelay = value;
        }
    }
}
