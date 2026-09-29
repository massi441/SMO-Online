using System.Runtime.InteropServices;
using Core.Memory;

namespace SMOO.Protocol;

/// <summary>
/// A strictly packet struct representing the header of a network packet. 
/// Every SMOO packet begins with this header.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal record struct PacketHeader : ISerializableStruct
{
    public readonly uint Magic = SMOOMagic;
    public required PacketType Type;
    public required byte Flags;
    public required byte Version;
    public required ushort RoomId;
    public ushort SequenceNumber;

    internal const uint SMOOMagic = 0x534D4F4F;
    internal const byte DefaultVersion = 1;

    public PacketHeader()
    {

    }

    /// <summary>
    /// Serializes the PacketHeader into a <see cref="SpanWriter", passed by reference/>
    /// </summary>
    /// <param name="writer">The SpanWriter to write the PacketHeader to.</param>
    public readonly void Serialize(ref SpanWriter writer)
    {
        writer.Write(this);
    }

    /// <summary>
    /// Creates a copy of the current PacketHeader with a new PacketType.
    /// </summary>
    /// <param name="type">The new PacketType.</param>
    /// <returns>A new PacketHeader with the specified PacketType.</returns>
    public PacketHeader WithType(PacketType type)
    {
        return this with { Type = type };
    }
}
