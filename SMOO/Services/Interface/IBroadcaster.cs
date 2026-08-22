using SMOO.Enumerator;
using SMOO.Server;
using SMOO.Memory;

namespace SMOO.Services.Interface;

/// <summary>
/// Broadcasts a message to a set of players
/// </summary>
internal interface IBroadcaster
{
    IReliablePacketStore ReliablePacketStore { get; }

    void Broadcast<TEnumerator>(ReadOnlySpan<byte> payload, TEnumerator players) where TEnumerator : IPlayerEnumerator<TEnumerator>, allows ref struct;
    void BroadcastReliably<TEnumerator>(SharedBuffer buffer, TEnumerator players, byte maxRetries = Constants.MaxRetries) where TEnumerator : IPlayerEnumerator<TEnumerator>, allows ref struct;
}
