using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SMOO.Event;

namespace SMOO.Protocol;

/// <summary>
/// Represents an event packet, ready to be processed by an <see cref="IEventHandler"/>
/// An event packet is an extenstion of a <see cref="RoomPacket"/>, with its own <see cref="EventHeader"/> and event data payload
/// </summary>
internal readonly struct EventPacket
{
    /// <summary>
    /// The base room packet 
    /// </summary>
    public required RoomPacket BasePacket { get; init; }

    /// <summary>
    /// Returns a view of the event header inside the packet's payload
    /// </summary>
    public ref EventHeader EventHeader => ref MemoryMarshal.AsRef<EventHeader>(BasePacket.Payload);

    /// <summary>
    /// Returns a span of the event data payload of the packet
    /// </summary>
    public Span<byte> EventData => BasePacket.Payload[Unsafe.SizeOf<EventHeader>()..];
}
