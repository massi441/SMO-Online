using Core.Memory;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SMOO.Protocol;

/// <summary>
/// Represents a network packet ready to be processed by the server
/// </summary>
internal readonly struct NetworkPacket
{
    /// <summary>
    /// The sender of the packet
    /// </summary>
    public required IPEndPoint Sender { get; init; }

    /// <summary>
    /// The rented buffer of the packet
    /// </summary>
    public required RentedBuffer Buffer { get; init; }

    /// <summary>
    /// Returns a view of the header inside the packet's payload
    /// </summary>
    public ref PacketHeader Header => ref MemoryMarshal.AsRef<PacketHeader>(Buffer.UsedSpan);

    /// <summary>
    /// The full size of the packet, including the header and payload
    /// </summary>
    public int FullSize => Buffer.UsedBytes;

    /// <summary>
    /// The size of the payload of the packet. (The size of the packet minus the size of the header)
    /// </summary>
    public int PayloadSize => Buffer.UsedBytes - Unsafe.SizeOf<PacketHeader>();
}
