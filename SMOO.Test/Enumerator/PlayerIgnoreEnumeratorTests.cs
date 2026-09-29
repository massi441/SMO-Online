using SMOO.Client;
using SMOO.Enumerator;
using SMOO.Test.Stubs;

namespace SMOO.Test.Enumerator;

public class PlayerIgnoreEnumeratorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void IgnoreEnumerator_SkipsIgnoredPlayer_AtAnyPosition(int ignoredSlot)
    {
        // Arrange
        Player[] players =
        [
            StubFactory.CreatePlayer(PlayerState.Connected, slot: 0),
            StubFactory.CreatePlayer(PlayerState.Connected, slot: 1),
            StubFactory.CreatePlayer(PlayerState.Connected, slot: 2)
        ];
        Player ignored = players[ignoredSlot];

        List<Player> expectedPlayers = [.. players.Where(player => player != ignored)];

        PlayerIgnoreEnumerator enumerator = new PlayerIgnoreEnumerator(players, ignored);

        // Act
        List<Player> enumerated = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Equal(expectedPlayers, enumerated);
    }

    [Fact]
    public void IgnoreEnumerator_SkipsDisconnectingPlayers()
    {
        // Arrange
        Player ignored = StubFactory.CreatePlayer(PlayerState.Connected, slot: 0);
        Player disconnecting = StubFactory.CreatePlayer(PlayerState.Disconnecting, slot: 1);
        Player connected = StubFactory.CreatePlayer(PlayerState.Connected, slot: 2);
        Player[] players = [ignored, disconnecting, connected];

        List<Player> expectedPlayers = [connected];

        PlayerIgnoreEnumerator enumerator = new PlayerIgnoreEnumerator(players, ignored);

        // Act
        List<Player> enumerated = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Equal(expectedPlayers, enumerated);
    }

    [Fact]
    public void IgnoreEnumerator_EnumeratesConnectingPlayers()
    {
        // Arrange
        Player ignored = StubFactory.CreatePlayer(PlayerState.Connected, slot: 0);
        Player connecting = StubFactory.CreatePlayer(PlayerState.Connecting, slot: 1);
        Player connected = StubFactory.CreatePlayer(PlayerState.Connected, slot: 2);
        Player[] players = [ignored, connecting, connected];

        List<Player> expectedPlayers = [connecting, connected];

        PlayerIgnoreEnumerator enumerator = new PlayerIgnoreEnumerator(players, ignored);

        // Act
        List<Player> enumerated = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Equal(expectedPlayers, enumerated);
    }

    [Fact]
    public void IgnoreEnumerator_SkipsEmptySlots()
    {
        // Arrange
        Player ignored = StubFactory.CreatePlayer(PlayerState.Connected, slot: 0);
        Player connected = StubFactory.CreatePlayer(PlayerState.Connected, slot: 2);
        Player[] players = [ignored, null!, connected, null!];

        List<Player> expectedPlayers = [connected];

        PlayerIgnoreEnumerator enumerator = new PlayerIgnoreEnumerator(players, ignored);

        // Act
        List<Player> enumerated = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Equal(expectedPlayers, enumerated);
    }

    [Fact]
    public void IgnoreEnumerator_EnumeratesNothing_WhenIgnoredIsTheOnlyPlayer()
    {
        // Arrange
        Player ignored = StubFactory.CreatePlayer(PlayerState.Connected, slot: 0);
        Player[] players = [ignored, null!];

        PlayerIgnoreEnumerator enumerator = new PlayerIgnoreEnumerator(players, ignored);

        // Act
        List<Player> enumerated = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Empty(enumerated);
    }

    [Fact]
    public void IgnoreEnumerator_EnumeratesEveryPlayer_WhenIgnoredIsNotInList()
    {
        // Arrange
        Player ignored = StubFactory.CreatePlayer(PlayerState.Connected);
        Player first = StubFactory.CreatePlayer(PlayerState.Connected, slot: 0);
        Player second = StubFactory.CreatePlayer(PlayerState.Connected, slot: 1);
        Player[] players = [first, second];

        List<Player> expectedPlayers = [first, second];

        PlayerIgnoreEnumerator enumerator = new PlayerIgnoreEnumerator(players, ignored);

        // Act
        List<Player> enumerated = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Equal(expectedPlayers, enumerated);
    }
}
