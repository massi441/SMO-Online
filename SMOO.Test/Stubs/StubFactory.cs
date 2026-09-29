using System.Net;
using Core.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SMOO.Client;
using SMOO.Server;
using SMOO.Services.Impl;
using SMOO.Services.Interface;

namespace SMOO.Test.Stubs;

internal static class StubFactory
{
    /// <summary>
    /// Creates a stub server context with everything 
    /// </summary>
    /// <returns></returns>
    public static ServerContext CreateContext()
    {
        return new ServerContext()
        {
            CancellationToken = CancellationToken.None,
            Logger = NullLogger.Instance,
            PacketController = Substitute.For<IPacketController>(),
            RoomHolder = Substitute.For<IRoomHolder>(),
        };
    }

    /// <summary>
    /// Creates a stub player with a given sequenced store
    /// </summary>
    public static Player CreatePlayerWithStore(SequencedPacketStore store)
    {
        return new Player()
        {
            Id = default(PlayerId),
            WorldInfo = new PlayerWorldInfo()
            {
                CostumeBody = string.Empty,
                CostumeCap = string.Empty,
                CurrentStage = string.Empty
            },
            SyncData = new PlayerSyncData(),
            Name = "Player",
            SequencedPacketStore = store,
            Room = Substitute.For<IRoom>(),
            Slot = 0
        };
    }

    /// <summary>
    /// Creates a stub player with optional parameters
    /// </summary>
    public static Player CreatePlayer(PlayerState state = PlayerState.Connecting, string stage = "", byte slot = 0, string name = "Player")
    {
        Player player = new Player()
        {
            Id = default(PlayerId),
            WorldInfo = new PlayerWorldInfo()
            {
                CostumeBody = string.Empty,
                CostumeCap = string.Empty,
                CurrentStage = stage
            },
            SyncData = new PlayerSyncData(),
            Name = name,
            SequencedPacketStore = Substitute.For<ISequencedPacketStore>(),
            Room = Substitute.For<IRoom>(),
            Slot = slot
        };

        if (state == PlayerState.Connected)
        {
            player.MarkConnected();
        }
        else if (state == PlayerState.Disconnecting)
        {
            player.MarkDisconnecting();
        }

        return player;
    }

    /// <summary>
    /// Creates a player registration request coming from a loopback endpoint on a given port
    /// </summary>
    public static PlayerRegisterInfo CreateRegisterInfo(int port = 5000, string name = "Player")
    {
        return new PlayerRegisterInfo()
        {
            Endpoint = new IPEndPoint(IPAddress.Loopback, port),
            Name = name,
            Room = Substitute.For<IRoom>()
        };
    }

    /// <summary>
    /// Creates an owned rented buffer with a default size of 1024
    /// </summary>
    public static RentedBuffer CreateBuffer(int size = 1024)
    {
        return new RentedBuffer(size);
    }
}
