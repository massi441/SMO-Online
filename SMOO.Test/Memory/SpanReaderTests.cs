using System.Runtime.InteropServices;
using SMOO.Memory;

namespace SMOO.Test.Memory;

public class SpanReaderTests
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private record struct PackedStruct
    {
        public required byte Byte;
        public required int Int32;
        public required float Single;
        public required double Double;
    }

    [Theory]
    [InlineData(0, 1, 2, 3, 4, 5)]
    [InlineData(10, 20, 5, -1, 8, -10)]
    [InlineData(-10, 35, -500, 1250, 1_000_000)]
    [InlineData(7, 77, 777, 7777)]
    [InlineData(42, 43)]
    public void ReadInt32_ReadsRightValues(params int[] numberStream)
    {
        // Arrange
        SpanReader reader = new SpanReader(MemoryMarshal.AsBytes(numberStream));

        // Act & Assert
        for (int i = 0; i < numberStream.Length; i++)
        {
            int expected = numberStream[i];
            int actual = reader.ReadInt32LittleEndian();

            Assert.Equal(expected, actual);
        }

        // Assert
        Assert.True(reader.IsDone);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    [InlineData(3, 1)]
    [InlineData(4, 3)]
    [InlineData(5, 5)]
    public void ReadInt32_AdvancesOffset(int totalInts, int numberOfReads)
    {
        // Arrange
        int expectedByteCount = (sizeof(int) * totalInts) - (sizeof(int) * numberOfReads);

        Span<int> stream = stackalloc int[totalInts];
        SpanReader reader = new SpanReader(MemoryMarshal.AsBytes(stream));

        // Act
        for (int i = 0; i < numberOfReads; i++)
        {
            reader.ReadInt32LittleEndian();
        }

        // Assert
        Assert.Equal(expectedByteCount, reader.RemainingByteCount);
    }

    [Theory]
    [InlineData((byte)10, 232, 10.2363f, 350.5364d)]
    [InlineData((byte)255, 1000000, -99.9999f, -1234.5678d)]
    [InlineData((byte)0, -500, 0.0f, 0.0d)]
    [InlineData((byte)127, -2147483648, 3.14159f, 999999.999d)]
    [InlineData((byte)42, 12345, 2.71828f, 0.00001d)]
    public void ReadGeneric_ReinterpretsBytes(byte byteData, int intData, float floatData, double doubleData)
    {
        // Arrange
        PackedStruct expected = new PackedStruct()
        {
            Byte = byteData,
            Int32 = intData,
            Single = floatData,
            Double = doubleData
        };

        ReadOnlySpan<byte> bytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref expected, 1));

        SpanReader reader = new SpanReader(bytes);

        // Act
        PackedStruct actual = reader.Read<PackedStruct>();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData((byte)10, 232, 10.2363f, 350.5364d)]
    [InlineData((byte)255, 1000000, -99.9999f, -1234.5678d)]
    [InlineData((byte)0, -500, -120.0f, 0.0d)]
    [InlineData((byte)127, -2147483648, 3.14159f, 999999.999d)]
    [InlineData((byte)42, 12345, 2.71828f, 0.00001d)]
    public void ReadGeneric_ReadsByCopy(byte byteData, int intData, float floatData, double doubleData)
    {
        // Arrange
        PackedStruct expected = new PackedStruct()
        {
            Byte = byteData,
            Int32 = intData,
            Single = floatData,
            Double = doubleData
        };

        ReadOnlySpan<byte> bytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref expected, 1));

        SpanReader reader = new SpanReader(bytes);

        // Act
        PackedStruct actual = reader.Read<PackedStruct>();

        expected.Byte = (byte)~expected.Byte;
        expected.Int32 = ~expected.Int32;
        expected.Single += 10.42f; 
        expected.Double += 10.45d;

        // Assert
        Assert.NotEqual(expected.Byte, actual.Byte);
        Assert.NotEqual(expected.Int32, actual.Int32);
        Assert.NotEqual(expected.Single, actual.Single);
        Assert.NotEqual(expected.Double, actual.Double);
    }
}
