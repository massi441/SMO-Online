using SMOO.Client;
using SMOO.Enumerator;
using SMOO.Test.Stubs;

namespace SMOO.Test.Enumerator;

public class PlayerSameStageEnumeratorTests
{
    private const string CapStage = "CapWorldHomeStage";
    private const string SandStage = "SandWorldHomeStage";

    [Fact]
    public void SameStageEnumerator_EnumeratesPlayersInTargetStage_ExcludingTarget()
    {
        // Arrange
        Player target = StubFactory.CreatePlayer(PlayerState.Connected, CapStage, slot: 0);
        Player sameStage = StubFactory.CreatePlayer(PlayerState.Connected, CapStage, slot: 1);
        Player otherStage = StubFactory.CreatePlayer(PlayerState.Connected, SandStage, slot: 2);
        Player otherSameStage = StubFactory.CreatePlayer(PlayerState.Connected, CapStage, slot: 3);
        Player[] players = [target, sameStage, otherStage, otherSameStage];

        List<Player> expectedPlayers = [sameStage, otherSameStage];

        PlayerSameStageEnumerator enumerator = new PlayerSameStageEnumerator(players, target);

        // Act
        List<Player> enumerated = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Equal(expectedPlayers, enumerated);
    }

    [Fact]
    public void SameStageEnumerator_SkipsDisconnectingPlayers()
    {
        // Arrange
        Player target = StubFactory.CreatePlayer(PlayerState.Connected, CapStage, slot: 0);
        Player disconnecting = StubFactory.CreatePlayer(PlayerState.Disconnecting, CapStage, slot: 1);
        Player connected = StubFactory.CreatePlayer(PlayerState.Connected, CapStage, slot: 2);
        Player[] players = [target, disconnecting, connected];

        List<Player> expectedPlayers = [connected];

        PlayerSameStageEnumerator enumerator = new PlayerSameStageEnumerator(players, target);

        // Act
        List<Player> enumerated = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Equal(expectedPlayers, enumerated);
    }

    [Fact]
    public void SameStageEnumerator_EnumeratesConnectingPlayers()
    {
        // Arrange
        Player target = StubFactory.CreatePlayer(PlayerState.Connected, CapStage, slot: 0);
        Player connecting = StubFactory.CreatePlayer(PlayerState.Connecting, CapStage, slot: 1);
        Player[] players = [target, connecting];

        List<Player> expectedPlayers = [connecting];

        PlayerSameStageEnumerator enumerator = new PlayerSameStageEnumerator(players, target);

        // Act
        List<Player> enumerated = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Equal(expectedPlayers, enumerated);
    }

    [Fact]
    public void SameStageEnumerator_SkipsEmptySlots()
    {
        // Arrange
        Player target = StubFactory.CreatePlayer(PlayerState.Connected, CapStage, slot: 0);
        Player sameStage = StubFactory.CreatePlayer(PlayerState.Connected, CapStage, slot: 2);
        Player[] players = [target, null!, sameStage, null!];

        List<Player> expectedPlayers = [sameStage];

        PlayerSameStageEnumerator enumerator = new PlayerSameStageEnumerator(players, target);

        // Act
        List<Player> enumerated = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Equal(expectedPlayers, enumerated);
    }

    [Fact]
    public void SameStageEnumerator_EnumeratesNothing_WhenAloneInStage()
    {
        // Arrange
        Player target = StubFactory.CreatePlayer(PlayerState.Connected, CapStage, slot: 0);
        Player otherStage = StubFactory.CreatePlayer(PlayerState.Connected, SandStage, slot: 1);
        Player[] players = [target, otherStage];

        PlayerSameStageEnumerator enumerator = new PlayerSameStageEnumerator(players, target);

        // Act
        List<Player> enumerated = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Empty(enumerated);
    }
}
