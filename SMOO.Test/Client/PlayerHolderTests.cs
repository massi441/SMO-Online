using Core.Memory;
using SMOO.Client;
using SMOO.Server;
using SMOO.Test.Stubs;

namespace SMOO.Test.Client;

public class PlayerHolderTests
{
    private const int BasePort = 5000;

    private readonly ServerContext _context;
    private readonly PlayerHolder _holder;

    public PlayerHolderTests()
    {
        _context = StubFactory.CreateContext();
        _holder = new PlayerHolder(_context);
    }

    [Fact]
    public void PlayerHolder_CreatesWithDefaultRoomSize()
    {
        // Assert
        Assert.Equal(PlayerHolder.DefaultRoomSize, _holder.MaxSize);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(PlayerHolder.MaxRoomSize, PlayerHolder.MaxRoomSize)]
    [InlineData(PlayerHolder.MaxRoomSize + 1, PlayerHolder.MaxRoomSize)]
    [InlineData(byte.MaxValue, PlayerHolder.MaxRoomSize)]
    public void PlayerHolder_LimitsSizeToMaxRoomSize(int size, int expectedSize)
    {
        // Arrange
        PlayerHolder holder = new PlayerHolder(_context, (byte)size);

        // Assert
        Assert.Equal(expectedSize, holder.MaxSize);
    }

    [Fact]
    public void PlayerHolder_RegistersPlayerFromRegisterInfo()
    {
        // Arrange
        PlayerRegisterInfo registerInfo = StubFactory.CreateRegisterInfo(BasePort, "Mario");

        // Act
        ServerResult<Player> result = _holder.RegisterPlayer(registerInfo);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(registerInfo.Endpoint, result.Data.Endpoint);
        Assert.Equal(registerInfo.Name, result.Data.Name);
        Assert.Same(registerInfo.Room, result.Data.Room);
        Assert.Same(result.Data, _holder.Players[result.Data.Slot]);
    }

    [Fact]
    public void PlayerHolder_RegistersPlayerWithDefaultState()
    {
        // Arrange
        PlayerRegisterInfo registerInfo = StubFactory.CreateRegisterInfo(BasePort);

        // Act
        Player player = _holder.RegisterPlayer(registerInfo).Data!;

        // Assert
        Assert.Equal(PlayerState.Connecting, player.State);
        Assert.Equal(string.Empty, player.WorldInfo.CurrentStage);
        Assert.Equal(PlayerHolder.DefaultCostumeName, player.WorldInfo.CostumeBody);
        Assert.Equal(PlayerHolder.DefaultCostumeName, player.WorldInfo.CostumeCap);
    }

    [Fact]
    public void PlayerHolder_AssignsUniqueSessionIds()
    {
        // Arrange
        PlayerRegisterInfo firstInfo = StubFactory.CreateRegisterInfo(BasePort);
        PlayerRegisterInfo secondInfo = StubFactory.CreateRegisterInfo(BasePort + 1);

        // Act
        Player first = _holder.RegisterPlayer(firstInfo).Data!;
        Player second = _holder.RegisterPlayer(secondInfo).Data!;

        // Assert
        Assert.NotEqual(Guid.Empty, first.Id.SessionId);
        Assert.NotEqual(first.Id.SessionId, second.Id.SessionId);
    }

    [Fact]
    public void PlayerHolder_AssignsFirstFreeSlots_InOrder()
    {
        // Arrange
        List<byte> expectedSlots = [0, 1, 2];

        // Act
        List<Player> players = RegisterPlayers(3);

        // Assert
        Assert.Equal(expectedSlots, players.Select(player => player.Slot));
    }

    [Fact]
    public void PlayerHolder_ReusesFreedSlot()
    {
        // Arrange
        List<Player> players = RegisterPlayers(3);
        Player leaving = players[1];
        PlayerRegisterInfo joiningInfo = StubFactory.CreateRegisterInfo(BasePort + 3);

        _holder.UnregisterPlayer(leaving);

        // Act
        Player joining = _holder.RegisterPlayer(joiningInfo).Data!;

        // Assert
        Assert.Equal(leaving.Slot, joining.Slot);
    }

    [Fact]
    public void PlayerHolder_RejectsPlayer_WithAlreadyRegisteredEndpoint()
    {
        // Arrange
        PlayerRegisterInfo registerInfo = StubFactory.CreateRegisterInfo(BasePort);
        PlayerRegisterInfo sameEndpointInfo = StubFactory.CreateRegisterInfo(BasePort);

        _holder.RegisterPlayer(registerInfo);

        // Act
        ServerResult<Player> result = _holder.RegisterPlayer(sameEndpointInfo);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal(ServerError.PlayerAlreadyInRoom, result.Error);
        Assert.Equal(1, _holder.Players.ActiveCount());
    }

    [Fact]
    public void PlayerHolder_RejectsPlayer_WhenRoomIsFull()
    {
        // Arrange
        RegisterPlayers(_holder.MaxSize);
        PlayerRegisterInfo extraInfo = StubFactory.CreateRegisterInfo(BasePort + _holder.MaxSize);

        // Act
        ServerResult<Player> result = _holder.RegisterPlayer(extraInfo);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal(ServerError.RoomFull, result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    public void PlayerHolder_UnregistersPlayer()
    {
        // Arrange
        Player player = RegisterPlayers(1)[0];

        // Act
        ServerResult result = _holder.UnregisterPlayer(player);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(_holder.Players[player.Slot]);
        Assert.Null(_holder.FindPlayerByHost(player.Endpoint));
    }

    [Fact]
    public void PlayerHolder_ReleasesPendingPackets_WhenUnregistering()
    {
        // Arrange
        Player player = RegisterPlayers(1)[0];

        using RentedBuffer buffer = StubFactory.CreateBuffer();

        player.SequencedPacketStore.UploadPacket(player, buffer);

        // Act
        _holder.UnregisterPlayer(player);

        // Assert
        Assert.Equal(1, buffer.RefCount);
    }

    [Fact]
    public void PlayerHolder_FailsToUnregister_WhenPlayerIsNotRegistered()
    {
        // Arrange
        Player player = StubFactory.CreatePlayer();

        // Act
        ServerResult result = _holder.UnregisterPlayer(player);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal(ServerError.PlayerNotFound, result.Error);
    }

    [Fact]
    public void PlayerHolder_FailsToUnregister_WhenAlreadyUnregistered()
    {
        // Arrange
        Player player = RegisterPlayers(1)[0];

        // Act
        ServerResult firstResult = _holder.UnregisterPlayer(player);
        ServerResult secondResult = _holder.UnregisterPlayer(player);

        // Assert
        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsFailed);
        Assert.Equal(ServerError.PlayerNotFound, secondResult.Error);
    }

    [Fact]
    public void PlayerHolder_FindsPlayerByHost()
    {
        // Arrange
        List<Player> players = RegisterPlayers(3);
        Player expectedPlayer = players[2];

        // Act
        Player? foundPlayer = _holder.FindPlayerByHost(expectedPlayer.Endpoint);

        // Assert
        Assert.Same(expectedPlayer, foundPlayer);
    }

    [Fact]
    public void PlayerHolder_DoesNotFindPlayerByHost_WhenHostIsNotRegistered()
    {
        // Arrange
        int playerCount = 2;

        RegisterPlayers(playerCount);
        PlayerRegisterInfo unknownInfo = StubFactory.CreateRegisterInfo(BasePort + playerCount);

        // Act
        Player? foundPlayer = _holder.FindPlayerByHost(unknownInfo.Endpoint);

        // Assert
        Assert.Null(foundPlayer);
    }

    [Fact]
    public void PlayerHolder_FindsPlayerById()
    {
        // Arrange
        List<Player> players = RegisterPlayers(3);
        Player expectedPlayer = players[2];

        // Act
        Player? foundPlayer = _holder.FindPlayerById(expectedPlayer.Id);

        // Assert
        Assert.Same(expectedPlayer, foundPlayer);
    }

    [Fact]
    public void PlayerHolder_DoesNotFindPlayerById_WhenSessionDiffers()
    {
        // Arrange
        Player player = RegisterPlayers(1)[0];
        PlayerId otherSessionId = new PlayerId()
        {
            Endpoint = player.Endpoint,
            SessionId = Guid.NewGuid()
        };

        // Act
        Player? foundPlayer = _holder.FindPlayerById(otherSessionId);

        // Assert
        Assert.Null(foundPlayer);
    }

    private List<Player> RegisterPlayers(int count)
    {
        List<Player> players = [];

        for (int i = 0; i < count; i++)
        {
            PlayerRegisterInfo registerInfo = StubFactory.CreateRegisterInfo(BasePort + i);
            Player player = _holder.RegisterPlayer(registerInfo).Data!;

            players.Add(player);
        }

        return players;
    }
}
