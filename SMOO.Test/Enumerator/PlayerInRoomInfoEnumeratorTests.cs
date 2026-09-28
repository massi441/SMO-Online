using SMOO.Client;
using SMOO.Enumerator;
using SMOO.Test.Stubs;

namespace SMOO.Test.Enumerator;

public class PlayerInRoomInfoEnumeratorTests
{
    [Fact]
    public void InRoomInfoEnumerator_MapsToRoomInfos()
    {
        // Arrange
        Player mario = StubFactory.CreatePlayer(PlayerState.Connected, slot: 0, name: "Mario");
        Player luigi = StubFactory.CreatePlayer(PlayerState.Connected, slot: 2, name: "Luigi");
        Player[] players = [mario, null!, luigi];

        PlayerInRoomInfo marioInfo = new PlayerInRoomInfo(mario);
        PlayerInRoomInfo luigiInfo = new PlayerInRoomInfo(luigi);

        List<PlayerInRoomInfo> expectedInfos = [marioInfo, luigiInfo];

        PlayerInRoomInfoEnumerator enumerator = new PlayerInRoomInfoEnumerator(players);

        // Act
        List<PlayerInRoomInfo> infos = ToRoomInfos(enumerator);

        // Assert
        Assert.Equal(expectedInfos, infos);
    }

    [Fact]
    public void InRoomInfoEnumerator_SkipsExcludedPlayer()
    {
        // Arrange
        Player excluded = StubFactory.CreatePlayer(PlayerState.Connecting, slot: 0, name: "Excluded");
        Player other = StubFactory.CreatePlayer(PlayerState.Connected, slot: 1, name: "Other");
        Player[] players = [excluded, other];

        PlayerInRoomInfo otherInfo = new PlayerInRoomInfo(other);

        List<PlayerInRoomInfo> expectedInfos = [otherInfo];

        PlayerInRoomInfoEnumerator enumerator = new PlayerInRoomInfoEnumerator(players, excluded);

        // Act
        List<PlayerInRoomInfo> infos = ToRoomInfos(enumerator);

        // Assert
        Assert.Equal(expectedInfos, infos);
    }

    [Fact]
    public void InRoomInfoEnumerator_IncludesConnectingPlayers()
    {
        // Arrange
        Player connecting = StubFactory.CreatePlayer(PlayerState.Connecting, slot: 0, name: "Connecting");
        Player connected = StubFactory.CreatePlayer(PlayerState.Connected, slot: 1, name: "Connected");
        Player[] players = [connecting, connected];

        PlayerInRoomInfo connectingInfo = new PlayerInRoomInfo(connecting);
        PlayerInRoomInfo connectedInfo = new PlayerInRoomInfo(connected);

        List<PlayerInRoomInfo> expectedInfos = [connectingInfo, connectedInfo];

        PlayerInRoomInfoEnumerator enumerator = new PlayerInRoomInfoEnumerator(players);

        // Act
        List<PlayerInRoomInfo> infos = ToRoomInfos(enumerator);

        // Assert
        Assert.Equal(expectedInfos, infos);
    }

    [Fact]
    public void InRoomInfoEnumerator_EnumeratesNothing_WhenAllSlotsAreEmpty()
    {
        // Arrange
        Player[] players = new Player[4];

        PlayerInRoomInfoEnumerator enumerator = new PlayerInRoomInfoEnumerator(players);

        // Act
        List<PlayerInRoomInfo> infos = ToRoomInfos(enumerator);

        // Assert
        Assert.Empty(infos);
    }

    private static List<PlayerInRoomInfo> ToRoomInfos(PlayerInRoomInfoEnumerator enumerator)
    {
        List<PlayerInRoomInfo> infos = [];

        foreach (PlayerInRoomInfo info in enumerator)
        {
            infos.Add(info);
        }

        return infos;
    }
}
