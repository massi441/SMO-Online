using SMOO.Enumerator;
using SMOO.Server;
using SMOO.Memory;

namespace SMOO.Services.Interface;

internal interface IBroadcaster
{
    /// <summary>
    /// The store of packets that need to be acked by clients
    /// </summary>
    IReliablePacketStore ReliablePacketStore { get; }
    void Broadcast<TEnumerator>(ReadOnlySpan<byte> payload, TEnumerator players) where TEnumerator : IPlayerEnumerator<TEnumerator>, allows ref struct;
    void BroadcastReliably<TEnumerator>(SharedBuffer buffer, TEnumerator players, byte maxRetries = Constants.MaxRetries) where TEnumerator : IPlayerEnumerator<TEnumerator>, allows ref struct;
    Task Shutdown();
}
