using Microsoft.Extensions.Logging;
using SMOO.Server;
using SMOO.Services.Interface;

namespace SMOO.Services.Impl;

internal class RoomMessageScheduler : IRoomMessageScheduler
{
    private readonly ServerContext _context;
    private Room _room = null!;
    private Task _packetResendTask = null!;
    private Task _playerHealthCheckTask = null!;
    private readonly CancellationTokenSource _scheduleTokenSource;

    public const int PacketResendDelay = 1000;
    public const int PlayerHealthCheckDelay = 1500;

    public RoomMessageScheduler(ServerContext context)
    {
        _context = context;
        _scheduleTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_context.CancellationToken);
    }

    public void Start(Room room)
    {
        _room = room;
        _packetResendTask = Task.Run(PacketResendLoop, _context.CancellationToken);
        _playerHealthCheckTask = Task.Run(PlayerHealthCheckLoop, _context.CancellationToken);

        _context.Logger.LogInformation("Starting message scheduler in Room #{RoomId}", room.Id);
    }

    public async Task Shutdown()
    {
        _scheduleTokenSource.Cancel();

        await Task.WhenAll(_packetResendTask, _playerHealthCheckTask);

        _scheduleTokenSource.Dispose();
    }

    private async Task PacketResendLoop()
    {
        while (!_scheduleTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(PacketResendDelay, _context.CancellationToken);

                _room.UploadMessage(RoomMessageType.PacketResend);
            }
            catch (Exception)
            {

            }
        }

        _context.Logger.LogInformation("Packet resend scheduler stopped successfully");
    }

    private async Task PlayerHealthCheckLoop()
    {
        while (!_scheduleTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(PlayerHealthCheckDelay, _context.CancellationToken);

                _room.UploadMessage(RoomMessageType.PlayerHealthCheck);
            }
            catch(Exception)
            {
                
            }
        }

        _context.Logger.LogInformation("Player health check scheduler stopped successfully");
    }
}
