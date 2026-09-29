using Core.Memory;
using SMOO.Enumerator;
using SMOO.Protocol;

namespace SMOO.Services.Interface;

/// <summary>
/// Broadcasts a message to a set of players
/// </summary>
internal interface IBroadcaster
{
    /// <summary>
    /// Broadcasts a message to a given set of players
    /// </summary>
    /// <typeparam name="TEnumerator">The type of the player set to broadcast to</typeparam>
    /// <param name="payload">The message to broadcast</param>
    /// <param name="players">The set of players to broadcast to</param>
    void Broadcast<TEnumerator>(ReadOnlySpan<byte> payload, TEnumerator players) where TEnumerator : IPlayerEnumerator<TEnumerator>, allows ref struct;

    /// <summary>
    /// Broadcasts a message to a given set of players reliably, ensuring the message is uplaoded to their sequenced store, with retries and delays
    /// </summary>
    /// <typeparam name="TEnumerator">The type of the player set to broadcast to</typeparam>
    /// <param name="buffer">The message to broadcast</param>
    /// <param name="players">The set of players to broadcast to</param>
    /// <param name="packetParams">The parameters for the sequenced packet</param>
    void BroadcastReliably<TEnumerator>(RentedBuffer buffer, TEnumerator players, SequencedPacketParams packetParams = default) where TEnumerator : IPlayerEnumerator<TEnumerator>, allows ref struct;
}
