using SMOO.Client;
using SMOO.Enumerator;
using SMOO.Server;
using SMOO.Services.Interface;
using SMOO.Memory;

namespace SMOO.Services.Impl;

internal class Broadcaster : IBroadcaster
{
    private readonly ServerContext _context;
    public IReliablePacketStore ReliablePacketStore { get; }

    public Broadcaster(ServerContext context, IReliablePacketStore reliablePacketStore)
    {
        _context = context;
        ReliablePacketStore = reliablePacketStore;
    }

    public void Broadcast<TEnumerator>(ReadOnlySpan<byte> payload, TEnumerator players) where TEnumerator : IPlayerEnumerator<TEnumerator>, allows ref struct
    {
        foreach (Player player in players)
        {
             _context.PacketController.Send(payload, player.Endpoint);
        }
    }

    public void BroadcastReliably<TEnumerator>(SharedBuffer buffer, TEnumerator players, byte maxRetries = Constants.MaxRetries) where TEnumerator : IPlayerEnumerator<TEnumerator>, allows ref struct
    {
        ReliablePacketStore.UploadBroadcast(buffer, players, maxRetries);

        foreach (Player player in players)
        {
            _context.PacketController.Send(buffer, player);
        }
    }
}
