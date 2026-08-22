using System.Net;
using Microsoft.Extensions.Logging;
using SMOO.Attributes;
using SMOO.Client;
using SMOO.Enumerator;
using SMOO.Protocol;
using SMOO.Serialization;
using SMOO.Server;
using SMOO.Memory;

namespace SMOO.Handle;

internal class PacketConnectSynHandler : IPacketHandler
{
    /// <summary>
    /// Requires at least one UInt16 for the length of the Player's name
    /// </summary>
    public static ushort MinPayloadSize => RequiredSize<PacketConnectPayload>.MinSize;
    public static ushort MaxPayloadSize => RequiredSize<PacketConnectPayload>.MaxSize;

    private struct PacketConnectPayload : IDeserializableStruct
    {
        [DynamicField(MaxSize = Constants.MaxPlayerNameLength)]
        public StreamStringView<byte> Name;

        public void Deserialize(ref SpanReader reader)
        {
            Name.Deserialize(ref reader, Constants.MaxPlayerNameLength);    
        }
    }

    public static void Handle(ParsedPacket packet, Room room, ServerContext context)
    {
        if (IsInOtherRoom(packet.SenderIp, context, out Player? takenPlayer, out Room takenRoom))
        {
            context.Logger.LogWarning("Player {Name} ({Address}:{Port}) is already in room {RoomId}", takenPlayer.Name, takenPlayer.Endpoint.Address, takenPlayer.Endpoint.Port, takenRoom.Id);
            return;
        }

        PacketConnectPayload connectPayload = PacketSerializer.Deserialize<PacketConnectPayload>(packet.Payload);

        if (!IsValidNameLength(connectPayload.Name.Length))
        {
            context.Logger.LogWarning("Invalid player name length {Length}", connectPayload.Name.Length);
            return;
        }

        PlayerInfo playerInfo = new PlayerInfo()
        {
            Endpoint = packet.SenderIp,
            Name = connectPayload.Name.String,
            Room = room,
        };

        ServerResult<Player> newPlayerResult = room.PlayerHolder.RegisterPlayer(in playerInfo);
        if (newPlayerResult.IsFailed)
        {
            context.Logger.LogError("Failed to register {PlayerName} in Room #{RoomId}", playerInfo.Name, room.Id);
            return;
        }

        Player newPlayer = newPlayerResult.Data!;

        PlayerInRoomInfoEnumerator playerInfos = room.Players.PlayerInfosExcept(newPlayer);

        PacketConnectSynAck ackPacket = new PacketConnectSynAck()
        {
            Header = packet.Header.WithType(PacketType.ConnectSynAck),
            RoomSize = room.PlayerHolder.MaxSize,
            SelfSlot = newPlayer.Slot,
            SessionId = newPlayer.Id.SessionId,
            OtherPlayersCount = (byte)(room.Players.ActiveCount() - 1),
            PlayerInfos = playerInfos
        };

        using SharedBuffer ackBuffer = PacketSerializer.SerializeShared(ref ackPacket, Constants.MaxBufferSize);

        context.PacketController.SendReliably(ackBuffer, newPlayer, room, resendDelay: Constants.PlayerSynAckDelay);

        context.Logger.LogTrace("Player {Name} joined Room #{RoomId} in slot {Slot}, waiting for a confirmation...", newPlayer.Name, packet.Header.RoomId, newPlayer.Slot);
    }

    // TODO: Figure out lightweight synchronization
    private static bool IsInOtherRoom(IPEndPoint sender, ServerContext context, out Player player, out Room takenRoom)
    {
        foreach (Room room in context.RoomHolder.GetRooms())
        {
            Player? p = room.PlayerHolder.FindPlayerByHost(sender);
            if (p != null)
            {
                player = p;
                takenRoom = room;
                return true;
            }
        }

        player = null!;
        takenRoom = null!;

        return false;
    }

    private static bool IsValidNameLength(int nameLength)
    {
        return nameLength > 0 && nameLength <= Constants.MaxPlayerNameLength;
    }
}
