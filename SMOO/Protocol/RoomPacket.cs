using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Core.Memory;
using SMOO.Client;
using SMOO.Handle;

namespace SMOO.Protocol;

/// <summary>
/// Represents a network packet with a validated header, ready to be processed by a <see cref="IPacketHandler"/>
/// </summary>
internal readonly struct RoomPacket
{
    public required IPEndPoint SenderIp { get; init; }
    public required RentedBuffer Buffer { get; init; }
    public Player? SenderPlayer { get; init; }

    /// <summary>
    /// Returns a view of the header inside the packet's payload
    /// </summary>
    public ref PacketHeader Header => ref MemoryMarshal.AsRef<PacketHeader>(Buffer.UsedSpan);

    /// <summary>
    /// Returns a span of the payload of the packet
    /// </summary>
    public Span<byte> Payload => Buffer.UsedSpan[Unsafe.SizeOf<PacketHeader>()..];

    /// <summary>
    /// The full size of the packet, including the header and payload
    /// </summary>
    public int FullSize => Buffer.UsedBytes;
}
