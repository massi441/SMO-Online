using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SMOO.Client;
using SMOO.Memory;

namespace SMOO.Protocol;

/// <summary>
/// Represents a network packet with a parsed header, ready to be handled by a server handler
/// </summary>
internal readonly struct ParsedPacket
{
    public required IPEndPoint SenderIp { get; init; }
    public required SharedBuffer Buffer { get; init; }
    public Player? SenderPlayer { get; init; }

    /// <summary>
    /// Returns a view of the header inside the packet's payload
    /// </summary>
    public ref PacketHeader Header => ref MemoryMarshal.AsRef<PacketHeader>(Buffer.UsedSpan);

    /// <summary>
    /// Returns a span of the payload of the packet
    /// </summary>
    public Span<byte> Payload => Buffer.UsedSpan[Unsafe.SizeOf<PacketHeader>()..];

    public int FullSize => Buffer.UsedBytes;
}
