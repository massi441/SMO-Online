using Microsoft.Extensions.Logging;
using SMOO.Client;
using SMOO.Enumerator;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Interface;
using SMOO.Threading;
using SMOO.Memory;

namespace SMOO.Services.Impl;

internal class Broadcaster : IBroadcaster
{
    private readonly ServerContext _context;
    private readonly IReliablePacketStore _reliablePacketStore;
    private readonly IPlayerHolder _playerHolder;
    private readonly CancellationTokenSource _resendToken;
    private readonly Task _resendTask;
    private readonly Stack<ReliablePacket> _deadPackets;

    public IReliablePacketStore ReliablePacketStore => _reliablePacketStore;

    public Broadcaster(ServerContext context, IReliablePacketStore reliablePacketStore, IPlayerHolder holder)
    {
        _context = context;
        _reliablePacketStore = reliablePacketStore;
        _playerHolder = holder;
        _resendToken = CancellationTokenSource.CreateLinkedTokenSource(_context.CancellationToken);
        _deadPackets = new Stack<ReliablePacket>();

        _resendTask = Task.Run(ResendLoop);
    }

    public void Broadcast<TEnumerator>(ReadOnlySpan<byte> payload, TEnumerator players) where TEnumerator : IPlayerEnumerator<TEnumerator>, allows ref struct
    {
        using ScopedReadLock readScope = _playerHolder.ReadWriteLock.EnterReadScope();

        foreach (Player player in players)
        {
             _context.PacketController.Send(payload, player.Endpoint);
        }
    }

    public void BroadcastReliably<TEnumerator>(SharedBuffer buffer, TEnumerator players, byte maxRetries = Constants.MaxRetries) where TEnumerator : IPlayerEnumerator<TEnumerator>, allows ref struct
    {
        using ScopedReadLock readScope = _playerHolder.ReadWriteLock.EnterReadScope();

        _reliablePacketStore.UploadBroadcast(buffer, players, maxRetries);

        foreach (Player player in players)
        {
            _context.PacketController.Send(buffer, player);
        }
    }

    private async Task ResendLoop()
    {
        try
        {
            while (!_resendToken.IsCancellationRequested)
            {
                await CheckPackets();
            }
        }
        finally
        {
            _resendToken.Dispose();
            _context.Logger.LogInformation("Room Broadcaster was shutdown successfully");
        }
    }

    private async Task CheckPackets()
    {
        _reliablePacketStore.PendingPackets.Lock();

        foreach (var playerPackets in _reliablePacketStore.PendingPackets)
        {
            foreach (var playerPacket in playerPackets.Value)
            {
                ReliablePacket reliablePacket = playerPacket.Value;
                if (reliablePacket.HasTriesLeft)
                {
                    TryResendPacket(reliablePacket);
                }
                else
                {
                    _deadPackets.Push(reliablePacket);
                }
            }
        }

        _reliablePacketStore.PendingPackets.Unlock();

        while (_deadPackets.TryPop(out ReliablePacket? deadPacket))
        {
            TryClearPacket(deadPacket);
        }

        try
        {
            await Task.Delay(Constants.ResendThreadTick, _resendToken.Token);
        }
        catch (OperationCanceledException)
        {
            _context.Logger.LogTrace("Broadcaster resend delay was cancelled, Room is shutting down...");
        }
    }

    private void TryResendPacket(ReliablePacket packet)
    {
        if (!packet.IsResendTime())
        {
            return;
        }

        _context.Logger.LogTrace("Resending {Type} packet #{Id} to {PlayerName} in room {#RoomdId}", packet.Header.Type, packet.SequenceNumber, packet.Receiver.Name, packet.Receiver.Room.Id);

        try
        {
            ServerResult sendResult = _context.PacketController.Send(packet.Buffer, packet.Receiver);
            if (!sendResult.IsSuccess)
            {
                _context.Logger.LogError("An error occured while trying to resend the packet: {Error}", sendResult.Error);
            }
        }
        catch(Exception ex)
        {
            _context.Logger.LogError("Failed to resend packet: {Message}", ex.Message);
        }

        packet.DecrementTries();
        packet.RefreshLastSent();
    }

    private void TryClearPacket(ReliablePacket reliablePacket)
    {
        ReliablePacket? expiredPacket = _reliablePacketStore.RemovePacket(reliablePacket.Receiver, reliablePacket.SequenceNumber);
        if (expiredPacket == null)
        {
            _context.Logger.LogWarning("Expired packet already removed");
            return;
        }

        ServerResult disconnectResult = _context.PlayerDisconnector.Disconnect(reliablePacket.Receiver);
        if (disconnectResult.IsSuccess)
        {
            _context.Logger.LogWarning("Disconnected player {PlayerName} for not acking packet (#{SequenceNumber}) in room #{RoomId}", reliablePacket.Receiver.Name, reliablePacket.SequenceNumber, reliablePacket.Receiver.Room.Id);
        }
        else
        {
            _context.Logger.LogError("Failed to disconnect player {PlayerName} for not Acking packet #{PacketId} in room #{RoomId}", reliablePacket.Receiver.Name, reliablePacket.SequenceNumber, reliablePacket.Receiver.Room.Id);
        }
    }

    public Task Shutdown()
    {
        _resendToken.Cancel();

        return _resendTask;
    }
}
