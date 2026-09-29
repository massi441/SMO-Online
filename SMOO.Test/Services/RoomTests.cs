using NSubstitute;
using SMOO.Client;
using SMOO.Services.Impl;
using SMOO.Services.Interface;
using SMOO.Test.Stubs;

namespace SMOO.Test.Services;

internal class RoomTests
{
    private readonly IPlayerHolder _playerHolder;
    private readonly IBroadcaster _broadcaster;
    private readonly IRoomMessageProcessorList _processorList;
    private readonly IRoomMessageScheduler _messageScheduler;
    private readonly IPlayerDisconnector _playerDisconnector;

    public RoomTests()
    {
        _playerHolder = Substitute.For<IPlayerHolder>();
        _broadcaster = Substitute.For<IBroadcaster>();
        _processorList = Substitute.For<IRoomMessageProcessorList>();
        _messageScheduler = Substitute.For<IRoomMessageScheduler>();
        _playerDisconnector = Substitute.For<IPlayerDisconnector>();
    }

    public void Room_CreatesWithId()
    {
        // Arrange
        int expectedId = 1;

        // Act
        Room room = BuildRoom(expectedId);

        // Assert
        Assert.Equal(expectedId, room.Id);
    }

    private Room BuildRoom(int id = 1)
    {
        return new Room(id, StubFactory.CreateContext(), _playerHolder, _broadcaster, _processorList, _messageScheduler, _playerDisconnector);
    }
}
