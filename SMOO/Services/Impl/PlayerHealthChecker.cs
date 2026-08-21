using Microsoft.Extensions.Logging;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Serialization;
using SMOO.Server;
using SMOO.Services.Interface;
using SMOO.Threading;
using SMOO.Memory;
using System.Runtime.CompilerServices;

namespace SMOO.Services.Impl;

internal class PlayerHealthChecker : IPlayerHealthChecker
{
    private readonly ServerContext _context;
    private readonly IPlayerHolder _playerHolder;
    private readonly Stack<Player> _disconnectedPlayers;
    private readonly Task _healthCheckTask;
    private readonly CancellationTokenSource _healthCheckToken;

    public PlayerHealthChecker(ServerContext context, IPlayerHolder playerHolder)
    {
        _context = context;
        _playerHolder = playerHolder;
        _disconnectedPlayers = new Stack<Player>(_playerHolder.MaxSize);

        _healthCheckToken = CancellationTokenSource.CreateLinkedTokenSource(_context.CancellationToken);
        _healthCheckTask = Task.Run(HealthCheckLoop);
    }

    public Task Shutdown()
    {
        _healthCheckToken.Cancel();

        return _healthCheckTask;
    }

    private async Task HealthCheckLoop()
    {
        try
        {
            while (!_healthCheckToken.IsCancellationRequested)
            {
                CheckIdlePlayers();

                try
                {
                    await Task.Delay(Constants.PlayerHealthCheckTick, _healthCheckToken.Token);
                }
                catch (OperationCanceledException)
                {
                    _context.Logger.LogTrace("Health check delay was cancelled, Room is shutting down...");
                }
            }
        }
        finally
        {
            _healthCheckToken.Dispose();
        }
    }

    private void CheckIdlePlayers()
    {
        using (_playerHolder.ReadWriteLock.EnterReadScope())
        {
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
