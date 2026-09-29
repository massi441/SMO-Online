using SMOO.Client;
using SMOO.Enumerator;
using SMOO.Test.Stubs;

namespace SMOO.Test.Enumerator;

public class PlayerInStateEnumeratorTests
{
    private readonly Player _connecting;
    private readonly Player _connected;
    private readonly Player _disconnecting;
    private readonly Player _otherConnected;
    private readonly Player _otherConnecting;
    private readonly Player[] _players; // contains a mix of players in different states

    public PlayerInStateEnumeratorTests()
    {
        _connecting = StubFactory.CreatePlayer(PlayerState.Connecting, slot: 1);
        _connected = StubFactory.CreatePlayer(PlayerState.Connected, slot: 3);
        _disconnecting = StubFactory.CreatePlayer(PlayerState.Disconnecting, slot: 4);
        _otherConnected = StubFactory.CreatePlayer(PlayerState.Connected, slot: 5);
        _otherConnecting = StubFactory.CreatePlayer(PlayerState.Connecting, slot: 7);

        _players = [null!, _connecting, null!, _connected, _disconnecting, _otherConnected, null!, _otherConnecting, null!];
    }

    [Fact]
    public void InStateEnumerator_EnumeratesConnectingPlayers()
    {
        // Arrange
        List<Player> expectedPlayers = [_connecting, _otherConnecting];

        PlayerInStateEnumerator enumerator = new PlayerInStateEnumerator(_players, PlayerState.Connecting);

        // Act
        List<Player> enumerated = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Equal(expectedPlayers, enumerated);
    }

    [Fact]
    public void InStateEnumerator_EnumeratesConnectedPlayers()
    {
        // Arrange
        List<Player> expectedPlayers = [_connected, _otherConnected];

        PlayerInStateEnumerator enumerator = new PlayerInStateEnumerator(_players, PlayerState.Connected);

        // Act
        List<Player> enumerated = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Equal(expectedPlayers, enumerated);
    }

    [Fact]
    public void InStateEnumerator_EnumeratesDisconnectingPlayers()
    {
        // Arrange
        List<Player> expectedPlayers = [_disconnecting];

        PlayerInStateEnumerator enumerator = new PlayerInStateEnumerator(_players, PlayerState.Disconnecting);

        // Act
        List<Player> enumerated = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Equal(expectedPlayers, enumerated);
    }

    [Fact]
    public void InStateEnumerator_EnumeratesNothing_WhenNoPlayerIsInState()
    {
        // Arrange
        Player[] players = [null!, _connected, null!, _otherConnected, null!];

        PlayerInStateEnumerator enumerator = new PlayerInStateEnumerator(players, PlayerState.Disconnecting);

        // Act
        List<Player> enumerated = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Empty(enumerated);
    }
}
