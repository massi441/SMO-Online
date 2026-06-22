using SMOO.Util;

namespace SMOO.Protocol;

internal static class PacketSerializer
{
    public static void SerializeScoped<T>(ref T packet, Span<byte> destination) where T : struct, ISerializableStruct, allows ref struct
    {
        SpanWriter writer = new SpanWriter(destination);

        packet.Serialize(ref writer);
    }

    public static SharedBuffer SerializeShared<T>(ref T packet, int requiredSize) where T : struct, ISerializableStruct, allows ref struct
    {
        SharedBuffer buffer = new SharedBuffer(requiredSize);
        SpanWriter writer = new SpanWriter(buffer);

        packet.Serialize(ref writer);
        buffer.Restrict(writer.Offset);

        return buffer;
    }

    public static T Deserialize<T>(ReadOnlySpan<byte> source) where T : struct, IDeserializableStruct, allows ref struct
    {
        T node = new T();
        SpanReader reader = new SpanReader(source);
        node.Deserialize(ref reader);
        return node;
    }

    public static T Deserialize<T>(ref SpanReader reader) where T : struct, IDeserializableStruct, allows ref struct
    {
        T node = new T();
        node.Deserialize(ref reader);
        return node;
    }
}
