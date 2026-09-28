using SMOO.Client;
using SMOO.Enumerator;
using SMOO.Test.Stubs;

namespace SMOO.Test.Enumerator;

public class PlayerActiveEnumeratorTests
{
    [Fact]
    public void ActiveEnumerator_EnumeratesNonNullPlayers_InSlotOrder()
    {
        // Arrange
        Player first = StubFactory.CreatePlayer(slot: 0);
        Player second = StubFactory.CreatePlayer(slot: 2);
        Player third = StubFactory.CreatePlayer(slot: 3);
        Player[] players = [first, null!, second, third, null!];

        List<Player> expectedPlayers = [first, second, third];

        PlayerActiveEnumerator enumerator = new PlayerActiveEnumerator(players);

        // Act
        List<Player> enumerated = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Equal(expectedPlayers, enumerated);
    }

    [Fact]
    public void ActiveEnumerator_EnumeratesNothing_WhenAllSlotsAreEmpty()
    {
        // Arrange
        Player[] players = new Player[4];

        PlayerActiveEnumerator enumerator = new PlayerActiveEnumerator(players);

        // Act
        List<Player> enumerated = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Empty(enumerated);
    }

    [Fact]
    public void ActiveEnumerator_EnumeratesNothing_WhenThereAreNoSlots()
    {
        // Arrange
        Player[] players = [];

        PlayerActiveEnumerator enumerator = new PlayerActiveEnumerator(players);

        // Act
        List<Player> enumerated = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Empty(enumerated);
    }

    [Fact]
    public void ActiveEnumerator_EnumeratesPlayersInEveryState()
    {
        // Arrange
        Player connecting = StubFactory.CreatePlayer(PlayerState.Connecting, slot: 0);
        Player connected = StubFactory.CreatePlayer(PlayerState.Connected, slot: 1);
        Player disconnecting = StubFactory.CreatePlayer(PlayerState.Disconnecting, slot: 2);
        Player[] players = [connecting, connected, disconnecting];

        List<Player> expectedPlayers = [connecting, connected, disconnecting];

        PlayerActiveEnumerator enumerator = new PlayerActiveEnumerator(players);

        // Act
        List<Player> enumerated = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Equal(expectedPlayers, enumerated);
    }

    [Fact]
    public void ActiveEnumerator_EnumeratesSamePlayersTwice()
    {
        // Arrange
        Player[] players = [StubFactory.CreatePlayer(slot: 0), StubFactory.CreatePlayer(slot: 1)];

        PlayerActiveEnumerator enumerator = new PlayerActiveEnumerator(players);

        // Act
        List<Player> firstPass = SMOTestUtil.GetEnumeratedPlayers(enumerator);
        List<Player> secondPass = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Equal(firstPass, secondPass);
    }
}
