using System.Runtime.InteropServices;
using SMOO.Memory;

namespace SMOO.Test.Memory;

public class SpanReaderTests
{
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
}
