using Microsoft.Extensions.Logging;
using SMOO.Client;
using SMOO.Enumerator;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Interface;
using SMOO.Threading;
using SMOO.MemUtil;

namespace SMOO.Services.Impl;

internal class Broadcaster : IBroadcaster
{
    private readonly ServerContext _context;
    private readonly IReliablePacketStore _resendStore;
    private readonly IPlayerHolder _playerHolder;
    private readonly CancellationTokenSource _resendToken;
    private readonly Task _resendTask;
    private readonly Stack<ReliablePacket> _deadPackets;

    public IReliablePacketStore ReliablePacketStore => _resendStore;

    public Broadcaster(ServerContext context, IReliablePacketStore resendStore, IPlayerHolder holder)
    {
        _context = context;
        _resendStore = resendStore;
        _playerHolder = holder;
        _resendToken = CancellationTokenSource.CreateLinkedTokenSource(_context.CancellationToken);
        _deadPackets = new Stack<ReliablePacket>();

        _resendTask = Task.Run(ResendLoop, _resendToken.Token);
    }

    public void Broadcast<TEnumerator>(ReadOnlySpan<byte> payload, TEnumerator players) where TEnumerator : IPlayerEnumerator<TEnumerator>, allows ref struct
    {
        using ScopedReadLock readScope = _playerHolder.ReadWriteLock.EnterReadScope();

        foreach (Player player in players)
        {
             _context.PacketSender.Send(payload, player.Endpoint);
        }
    }

    public void BroadcastReliably<TEnumerator>(SharedBuffer buffer, TEnumerator players, byte maxRetries = Constants.MaxRetries) where TEnumerator : IPlayerEnumerator<TEnumerator>, allows ref struct
    {
        using ScopedReadLock readScope = _playerHolder.ReadWriteLock.EnterReadScope();

        foreach (Player player in players)
        {
            _resendStore.UploadPacket(buffer, player, maxRetries);
            _context.PacketSender.Send(buffer, player);
        }
    }

    private async Task ResendLoop()
    {
        while (!_resendToken.IsCancellationRequested)
        {
            await CheckPackets();
        }

        _context.Logger.LogInformation("Room Broadcaster was shutdown successfully");
    }

    private async Task CheckPackets()
    {
        _resendStore.PendingPackets.Lock();

        foreach (var pair in _resendStore.PendingPackets)
        {
            ReliablePacket packet = pair.Value;

            if (packet.HasTriesLeft)
            {
                TryResendPacket(packet);
            }
            else
            {
                _deadPackets.Push(packet);
            }
        }

        _resendStore.PendingPackets.Unlock();

        while (_deadPackets.TryPop(out ReliablePacket? deadPacket))
        {
            TryClearPacket(deadPacket);
        }

        await Task.Delay(Constants.ResendThreadTick);
    }

    private void TryResendPacket(ReliablePacket packet)
    {
        if (!packet.IsResendTime())
        {
            return;
        }

        _context.Logger.LogTrace("Resending {Type} packet #{Id} to {PlayerName} in room {#RoomdId}", packet.Header.Type, packet.SequenceNumber, packet.Receiver.Name, packet.Receiver.Room.Id);

        packet.WriteSequenceNumber(); // write the packet's sequence number into the payload in case the buffer is shared

        ServerResult sendResult = _context.PacketSender.Send(packet.Buffer, packet.Receiver);
        if (!sendResult.IsSuccess)
        {
            _context.Logger.LogError("An error occured while trying to resend the packet");
        }

        packet.DecrementTries();
        packet.RefreshLastSent();
    }

    private void TryClearPacket(ReliablePacket reliablePacket)
    {
        PacketType packetType = reliablePacket.Header.Type; // need to capture here as packet store frees the rented buffer

        ReliablePacket? expiredPacket = _resendStore.RemovePacket(reliablePacket.Receiver, reliablePacket.SequenceNumber);
        if (expiredPacket == null)
        {
            _context.Logger.LogWarning("Expired packet already removed");
            return;
        }

        ServerResult disconnectResult = _context.PlayerDisconnector.Disconnect(reliablePacket.Receiver);
        if (disconnectResult.IsSuccess)
        {
            _context.Logger.LogWarning("Disconnected player {PlayerName} for not Acking {PacketType} packet (#{SequenceNumber}) in room #{RoomId}", reliablePacket.Receiver.Name, packetType, reliablePacket.SequenceNumber, reliablePacket.Receiver.Room.Id);
        }
        else
        {
            _context.Logger.LogError("Failed to disconnect player {PlayerName} for not Acking packet #{PacketId} in room #{RoomId}", reliablePacket.Receiver.Name, reliablePacket.SequenceNumber, reliablePacket.Receiver.Room.Id);
        }
    }

    public Task Shutdown()
    {
        _resendToken.Cancel();
        _resendToken.Dispose();

        return _resendTask;
    }
}
