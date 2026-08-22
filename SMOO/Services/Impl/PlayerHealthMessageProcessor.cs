using Microsoft.Extensions.Logging;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Serialization;
using SMOO.Server;
using SMOO.Memory;
using SMOO.Services.Interface;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace SMOO.Services.Impl;

/// <summary>
/// Sends health check packets to players that have been idle for too long, and disconnects
/// players that are unresponsive.
/// </summary>
internal class PlayerHealthMessageProcessor : IRoomMessageProcessor
{
    private readonly ServerContext _context;
    private readonly IPlayerHolder _playerHolder;
    private readonly Stack<Player> _disconnectedPlayers;

    public PlayerHealthMessageProcessor(ServerContext context, IPlayerHolder playerHolder)
    {
        _context = context;
        _playerHolder = playerHolder;
        _disconnectedPlayers = new Stack<Player>(_playerHolder.MaxSize);
    }

    public void Process(Room room, Packet packet)
    {
        Debug.Assert(room.PlayerHolder == _playerHolder, "PlayerHolder in health check is different from the room it is processing");

        foreach (Player player in _playerHolder.Players)
        {
            if (player.IsConnectionLost())
            {
                _disconnectedPlayers.Push(player);
                _context.Logger.LogWarning("Player {PlayerName} has lost connection in Room #{RoomId} and will be disconnected", player.Name, player.Room.Id);
                continue;
            }

            if (player.IsNeedHealthCheck())
            {
                SendHealthCheck(player);
            }
        }

        while (_disconnectedPlayers.TryPop(out Player? disconnectedPlayer))
        {
            DisconnectPlayer(disconnectedPlayer);
        }
    }

    private void SendHealthCheck(Player player)
    {
        PacketHeader header = new PacketHeader()
        {
            Type = PacketType.HealthCheck,
            Flags = 0,
            Version = Constants.Version,
            RoomId = player.Room.Id
        };

        using SharedBuffer buffer = PacketSerializer.SerializeShared(ref header, Unsafe.SizeOf<PacketHeader>());

        _context.Logger.LogTrace("Player {PlayerName} has been idle for too long in Room #{RoomId}, a health check request will be sent", player.Name, player.Room.Id);

        try
        {
            _context.PacketController.Send(buffer, player);
        }
        catch (Exception ex)
        {
            _context.Logger.LogError("An error occured while sending health check to {PlayerName}: {Message}", player.Name, ex.Message);
        }

    }

    private void DisconnectPlayer(Player player)
    {
        ServerResult disconnectResult = _context.PlayerDisconnector.Disconnect(player);
        if (disconnectResult.IsSuccess)
        {
            _context.Logger.LogInformation("Successfully disconnected {PlayerName} from Room #{RoomId}", player.Name, player.Room.Id);
        }
        else
        {
            _context.Logger.LogError("Failed to disconnect player {PlayerName} in Room #{RoomId}: {Error}", player.Name, player.Room.Id, disconnectResult.Error!.Value);
        }
    }
}
