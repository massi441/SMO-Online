using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SMOO.Util;

namespace SMOO.Protocol;

/// <summary>
/// Represents a network packet ready to be processed by the server
/// </summary>
internal readonly struct Packet
{
    /// <summary>
    /// The sender of the packet
    /// </summary>
    public required IPEndPoint Sender { get; init; }

    /// <summary>
    /// The rented buffer of the packet
    /// </summary>
    public required SharedBuffer Buffer { get; init; }

    /// <summary>
    /// Returns a view of the header inside the packet's payload
    /// </summary>
    public ref PacketHeader Header => ref MemoryMarshal.AsRef<PacketHeader>(Buffer.UsedSpan);

    /// <summary>
    /// The full size of the packet
    /// </summary>
    public int FullSize => Buffer.UsedBytes;

    public int PayloadSize => Buffer.UsedBytes - Unsafe.SizeOf<PacketHeader>();
}
