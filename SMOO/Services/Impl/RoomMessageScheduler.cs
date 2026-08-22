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

        _packetResendTask = ScheduleMessage(RoomMessageType.PacketResend, PacketResendDelay);
        _playerHealthCheckTask = ScheduleMessage(RoomMessageType.PlayerHealthCheck, PlayerHealthCheckDelay);

        _context.Logger.LogInformation("Starting message scheduler in Room #{RoomId}", room.Id);
    }

    /// <summary>
    /// Shutdowns the current scheduler by waiting for all scheduled messagers to be done
    /// </summary>
    /// <returns>A task that waits for all message schedulers to be done</returns>
    public async Task Shutdown()
    {
        _scheduleTokenSource.Cancel();

        await Task.WhenAll(_packetResendTask, _playerHealthCheckTask);

        _scheduleTokenSource.Dispose();
    }

    /// <summary>
    /// Schedules a message that is periodically uploaded to the room
    /// </summary>
    /// <param name="messageType">The type of message uploaded</param>
    /// <param name="delayMs">The delay between each message in MS</param>
    /// <returns>A task that perdiodically uploads a message to the room</returns>
    private async Task ScheduleMessage(RoomMessageType messageType, int delayMs)
    {
        using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMilliseconds(delayMs));

        try
        {
            while (await timer.WaitForNextTickAsync(_scheduleTokenSource.Token))
            {
                _room.UploadMessage(messageType);
            }
        }
        catch (OperationCanceledException)
        {

        }
        catch (Exception ex)
        {
            _context.Logger.LogError(ex, "{MessageType} scheduler failed in room #{RoomId}", messageType, _room.Id);
            return;
        }

        _context.Logger.LogInformation("{MessageType} scheduler stopped successfully in room #{RoomId}", messageType, _room.Id);
    }
}
