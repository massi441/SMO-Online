using Core.Memory;

namespace SMOO.Protocol;

/// <summary>
/// A static class that provides methods for serializing and deserializing packets.
/// </summary>
internal static class PacketSerializer
{
    /// <summary>
    /// Serializes a packet of type T into the provided destination span.
    /// </summary>
    /// <typeparam name="T">The type of the packet to serialize</typeparam>
    /// <param name="packet">The packet to serialize</param>
    /// <param name="destination">The buffer to serialize the packet into</param>
    public static void SerializeScoped<T>(ref T packet, Span<byte> destination) where T : struct, ISerializableStruct, allows ref struct
    {
        SpanWriter writer = new SpanWriter(destination);

        packet.Serialize(ref writer);
    }

    /// <summary>
    /// Serializes a packet of type T into a rented buffer of the specified size.
    /// Ensures the buffer is restricted to the amount of bytes written after serialization.
    /// </summary>
    /// <typeparam name="T">The type of packet to serialize</typeparam>
    /// <param name="packet">The packet to serialize</param>
    /// <param name="requiredSize">The size of the buffer to rent</param>
    /// <returns>The rented buffer containing the serialized packet</returns>
    public static RentedBuffer SerializeRent<T>(ref T packet, int requiredSize) where T : struct, ISerializableStruct, allows ref struct
    {
        RentedBuffer buffer = new RentedBuffer(requiredSize);
        SpanWriter writer = new SpanWriter(buffer);

        packet.Serialize(ref writer);
        buffer.Restrict(writer.Offset);

        return buffer;
    }

    /// <summary>
    /// Deserializes a packet of type T from the provided source span.
    /// </summary>
    /// <typeparam name="T">The type of the packet to deserialize</typeparam>
    /// <param name="source">The span to deserialize the packet from</param>
    /// <returns>The deserialized packet</returns>
    public static T Deserialize<T>(ReadOnlySpan<byte> source) where T : struct, IDeserializableStruct, allows ref struct
    {
        T node = new T();
        SpanReader reader = new SpanReader(source);
        node.Deserialize(ref reader);
        return node;
    }

    /// <summary>
    /// Deserializes a packet of type T from the provided SpanReader.
    /// </summary>
    /// <typeparam name="T">The type of the packet to deserialize</typeparam>
    /// <param name="reader">The SpanReader to deserialize the packet from</param>
    /// <returns>The deserialized packet</returns>
    public static T Deserialize<T>(ref SpanReader reader) where T : struct, IDeserializableStruct, allows ref struct
    {
        T node = new T();
        node.Deserialize(ref reader);
        return node;
    }
}
