using SMOO.Client;
using SMOO.Enumerator;
using SMOO.Test.Stubs;

namespace SMOO.Test.Enumerator;

public class SpanEnumeratorExtensionsTests
{
    private const string CapStage = "CapWorldHomeStage";

    [Fact]
    public void Count_ReturnsNumberOfEnumeratedPlayers()
    {
        // Arrange
        Player[] players = [StubFactory.CreatePlayer(slot: 0), null!, StubFactory.CreatePlayer(slot: 2)];

        PlayerActiveEnumerator enumerator = new PlayerActiveEnumerator(players);

        // Act
        int count = enumerator.Count<Player, PlayerActiveEnumerator>();

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public void Count_ReturnsZero_WhenNothingIsEnumerated()
    {
        // Arrange
        Player[] players = new Player[4];

        PlayerActiveEnumerator enumerator = new PlayerActiveEnumerator(players);

        // Act
        int count = enumerator.Count<Player, PlayerActiveEnumerator>();

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public void Count_DoesNotConsumeEnumerator()
    {
        // Arrange
        Player target = StubFactory.CreatePlayer(PlayerState.Connected, CapStage, slot: 0);
        Player first = StubFactory.CreatePlayer(PlayerState.Connected, CapStage, slot: 1);
        Player second = StubFactory.CreatePlayer(PlayerState.Connected, CapStage, slot: 2);
        Player[] players = [target, first, second];

        List<Player> expectedPlayers = [first, second];

        PlayerSameStageEnumerator enumerator = new PlayerSameStageEnumerator(players, target);

        // Act
        int count = enumerator.Count<Player, PlayerSameStageEnumerator>();
        List<Player> enumerated = SMOTestUtil.GetEnumeratedPlayers(enumerator);

        // Assert
        Assert.Equal(2, count);
        Assert.Equal(expectedPlayers, enumerated);
    }
}
