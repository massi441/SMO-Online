using SMOO.Client;
using SMOO.Enumerator;
using SMOO.Test.Stubs;

namespace SMOO.Test.Enumerator;

public class PlayerNotInStateEnumeratorTests
{
    private readonly Player _connecting;
    private readonly Player _connected;
    private readonly Player _disconnecting;
    private readonly Player _otherConnected;
    private readonly Player _otherConnecting;
    private readonly Player[] _players; // contains a list of players in the different states

    public PlayerNotInStateEnumeratorTests()
    {
        _connecting = StubFactory.CreatePlayer(PlayerState.Connecting, slot: 1);
        _connected = StubFactory.CreatePlayer(PlayerState.Connected, slot: 3);
        _disconnecting = StubFactory.CreatePlayer(PlayerState.Disconnecting, slot: 4);
        _otherConnected = StubFactory.CreatePlayer(PlayerState.Connected, slot: 5);
        _otherConnecting = StubFactory.CreatePlayer(PlayerState.Connecting, slot: 7);

        _players = [null!, _connecting, null!, _connected, _disconnecting, _otherConnected, null!, _otherConnecting, null!];
    }

    [Fact]
    public void NotInStateEnumerator_SkipsDisconnectingPlayers()
    {
        // Arrange
        List<Player> expectedPlayers = [_connecting, _connected, _otherConnected, _otherConnecting];

        PlayerNotInStateEnumerator enumerator = new PlayerNotInStateEnumerator(_players, PlayerState.Disconnecting);

        // Act
        List<Player> enumerated = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Equal(expectedPlayers, enumerated);
    }

    [Fact]
    public void NotInStateEnumerator_SkipsConnectedPlayers()
    {
        // Arrange
        List<Player> expectedPlayers = [_connecting, _disconnecting, _otherConnecting];

        PlayerNotInStateEnumerator enumerator = new PlayerNotInStateEnumerator(_players, PlayerState.Connected);

        // Act
        List<Player> enumerated = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Equal(expectedPlayers, enumerated);
    }

    [Fact]
    public void NotInStateEnumerator_SkipsConnectingPlayers()
    {
        // Arrange
        List<Player> expectedPlayers = [_connected, _disconnecting, _otherConnected];

        PlayerNotInStateEnumerator enumerator = new PlayerNotInStateEnumerator(_players, PlayerState.Connecting);

        // Act
        List<Player> enumerated = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Equal(expectedPlayers, enumerated);
    }

    [Fact]
    public void NotInStateEnumerator_EnumeratesEveryPlayer_WhenNoPlayerIsInState()
    {
        // Arrange
        Player[] players = [null!, _connecting, null!, _connected, null!];

        List<Player> expectedPlayers = [_connecting, _connected];

        PlayerNotInStateEnumerator enumerator = new PlayerNotInStateEnumerator(players, PlayerState.Disconnecting);

        // Act
        List<Player> enumerated = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Equal(expectedPlayers, enumerated);
    }
}
