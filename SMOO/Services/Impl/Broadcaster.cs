using Core.Memory;
using SMOO.Client;
using SMOO.Enumerator;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Interface;

namespace SMOO.Services.Impl;

internal class Broadcaster : IBroadcaster
{
    private readonly ServerContext _context;

    public Broadcaster(ServerContext context)
    {
        _context = context;
    }

    public void Broadcast<TEnumerator>(ReadOnlySpan<byte> payload, TEnumerator players) where TEnumerator : IPlayerEnumerator<TEnumerator>, allows ref struct
    {
        foreach (Player player in players)
        {
             _context.PacketController.Send(payload, player.Endpoint);
        }
    }

    public void BroadcastReliably<TEnumerator>(RentedBuffer buffer, TEnumerator players, SequencedPacketParams packetParams = default) where TEnumerator : IPlayerEnumerator<TEnumerator>, allows ref struct
    {
        foreach (Player player in players)
        {
            _context.PacketController.SendReliably(buffer, player, packetParams);
        }
    }
}
