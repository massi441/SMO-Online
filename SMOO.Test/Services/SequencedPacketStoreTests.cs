using Core.Memory;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Impl;
using SMOO.Test.Stubs;

namespace SMOO.Test.Services;

public class SequencedPacketStoreTests : IDisposable
{
    private readonly RentedBuffer _buffer;
    private readonly SequencedPacketStore _store;
    private readonly Player _player;

    public SequencedPacketStoreTests()
    {
        _buffer = StubFactory.CreateBuffer();
        _store = new SequencedPacketStore(StubFactory.CreateContext());
        _player = StubFactory.CreatePlayerWithStore(_store);
    }

    [Fact]
    public void SequencedStore_CreatesWithMaxStoreSize()
    {
        // Arrange
        int expectedSize = SequencedPacketStore.MaxStoreSize;

        // Assert
        Assert.Equal(expectedSize, _store.Capacity);
    }

    [Fact]
    public void SequencedStore_UploadsAndReturnsPacket()
    {
        // Act
        ServerResult<SequencedPacket> packetResult = _store.UploadPacket(_player, _buffer);

        // Assert
        Assert.True(packetResult.IsSuccess);
        Assert.NotNull(packetResult.Data);
        Assert.Equal(0, packetResult.Data.SequenceNumber);
    }

    [Fact]
    public void SequencedStore_AcquiresBufferReference()
    {
        // Assert & Act
        Assert.Equal(1, _buffer.RefCount);

        ServerResult<SequencedPacket> packetResult = _store.UploadPacket(_player, _buffer);

        // Assert
        Assert.Equal(_buffer, packetResult.Data!.Buffer);
        Assert.Equal(2, packetResult.Data!.Buffer.RefCount);
    }

    [Fact]
    public void SequencedStore_RemovesPacket()
    {
        // Arrange
        ServerResult<SequencedPacket> packet = _store.UploadPacket(_player, _buffer);

        // Act
        SequencedPacket? removedPacket = _store.RemovePacket(packet.Data!.SequenceNumber);

        // Assert
        Assert.NotNull(removedPacket);
        Assert.Equal(packet.Data!, removedPacket);
    }

    [Fact]
    public void SequencedStore_RemovesPacket_Idempotently()
    {
        // Arrange
        ServerResult<SequencedPacket> packet = _store.UploadPacket(_player, _buffer);

        // Act
        SequencedPacket? _ = _store.RemovePacket(packet.Data!.SequenceNumber);
        SequencedPacket? removedPacket2 = _store.RemovePacket(packet.Data!.SequenceNumber);

        // Assert
        Assert.Null(removedPacket2);
    }

    [Fact]
    public void SequencedStore_FillsStore()
    {
        // Act & Assert
        for (int i = 0; i < _store.Capacity; i++)
        {
            using RentedBuffer buffer = StubFactory.CreateBuffer();

            ServerResult<SequencedPacket> packetResult = _store.UploadPacket(_player, buffer);

            Assert.True(packetResult.IsSuccess);
            Assert.Equal(i, packetResult.Data!.SequenceNumber);
            Assert.Equal(buffer, packetResult.Data!.Buffer);
            Assert.Equal(2, buffer.RefCount);
        }
    }

    [Fact]
    public void SequencedStore_RejectsPacket_WhenFull()
    {
        // Arrange
        for (int i = 0; i < _store.Capacity; i++)
        {
            using RentedBuffer buffer = StubFactory.CreateBuffer();
            _store.UploadPacket(_player, buffer);
        }

        // Act
        ServerResult<SequencedPacket> packetResult = _store.UploadPacket(_player, _buffer);

        // Assert
        Assert.True(packetResult.IsFailed);
        Assert.Equal(ServerError.PendingPacketStoreFull, packetResult.Error);
        Assert.Null(packetResult.Data);
        Assert.Equal(1, _buffer.RefCount);
    }

    [Fact]
    public void SequencedStore_Clear_ReleasesBuffers()
    {
        // Arrange
        SequencedPacket[] packets = new SequencedPacket[_store.Capacity];

        for (int i = 0; i < _store.Capacity; i++)
        {
            using RentedBuffer buffer = StubFactory.CreateBuffer(); // ref count goes to 1 after the disposal here, so only the store keeps the buffer alive
            ServerResult<SequencedPacket> packet = _store.UploadPacket(_player, buffer);

            Assert.NotNull(packet.Data);

            packets[i] = packet.Data!;
        }

        // Act
        int clearCount = _store.Clear();

        Assert.Equal(packets.Length, clearCount);

        // Assert
        for (int i = 0; i < packets.Length; i++)
        {
            SequencedPacket packet = packets[i];
            SequencedPacket? removedPacket = _store.RemovePacket(packet.SequenceNumber);

            Assert.Null(removedPacket); // it should already be removed by Clear()
            Assert.True(packet.Buffer.IsReleased());
        }
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(SequencedPacketStore.MaxStoreSize - 1, SequencedPacketStore.MaxStoreSize - 1)]
    [InlineData(SequencedPacketStore.MaxStoreSize, SequencedPacketStore.MaxStoreSize)]
    [InlineData(ushort.MaxValue, ushort.MaxValue)]
    [InlineData(ushort.MaxValue + 1, 0)]
    public void SequencedStore_AssignsSequenceNumber_AfterFilling(int fillCount, int expectedSequenceNumber)
    {
        // Arrange
        SMOTestUtil.AdvanceStore(_store, _player, fillCount);

        // Act
        ServerResult<SequencedPacket> packetResult = _store.UploadPacket(_player, _buffer);

        // Assert
        Assert.True(packetResult.IsSuccess);
        Assert.Equal(expectedSequenceNumber, packetResult.Data!.SequenceNumber);
        Assert.Equal(expectedSequenceNumber, packetResult.Data!.Header.SequenceNumber);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(17, 2)]
    [InlineData(SequencedPacketStore.MaxStoreSize - 1, 1)]
    [InlineData(ushort.MaxValue, 1)]
    public void SequencedStore_Remove_RejectsSlotWithDifferentSequenceNumber(int sequenceNumber, int wrapCount)
    {
        // Arrange
        SMOTestUtil.AdvanceStore(_store, _player, sequenceNumber);

        _store.UploadPacket(_player, _buffer);
        ushort wrappedSequenceNumber = (ushort)(sequenceNumber + wrapCount * _store.Capacity); // ends in the same slot, but doesn't have the same sequence number

        // Act
        SequencedPacket? removedPacket = _store.RemovePacket(wrappedSequenceNumber);

        // Assert
        Assert.Null(removedPacket);
    }

    // TODO: Test ResendPackets

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _buffer.Release();
    }
}
