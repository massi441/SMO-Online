using Core.Memory;
using SMOO.Enumerator;
using SMOO.Protocol;

namespace SMOO.Services.Interface;

/// <summary>
/// Broadcasts a message to a set of players
/// </summary>
internal interface IBroadcaster
{
    void Broadcast<TEnumerator>(ReadOnlySpan<byte> payload, TEnumerator players) where TEnumerator : IPlayerEnumerator<TEnumerator>, allows ref struct;
    void BroadcastReliably<TEnumerator>(RentedBuffer buffer, TEnumerator players, SequencedPacketParams packetParams = default) where TEnumerator : IPlayerEnumerator<TEnumerator>, allows ref struct;
}
