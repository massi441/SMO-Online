using SMOO.Client;
using SMOO.Enumerator;
using SMOO.Memory;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Threading;

namespace SMOO.Services.Interface;

internal interface IReliablePacketStore
{
    LockedDictionary<Player, Dictionary<ushort, ReliablePacket>> PendingPackets { get; }

    ReliablePacket UploadPacket(SharedBuffer buffer, Player receiver, byte maxRetries = Constants.MaxRetries, int resendDelay = Constants.DefaultResendDelay);
    void UploadBroadcast<TEnumerator>(SharedBuffer buffer, TEnumerator players, byte maxRetries = Constants.MaxRetries, int resendDelay = Constants.DefaultResendDelay) where TEnumerator : IPlayerEnumerator<TEnumerator>, allows ref struct;

    void ClearPlayer(Player player);

    /// <summary>
    /// Removes a reliable packet, and returns its rented buffer to the array pool.
    /// </summary>
    /// <param name="sequenceNumber">The sequence number of the packet to remove</param>
    /// <returns>The removed packed</returns>
    ReliablePacket? RemovePacket(Player requester, ushort sequenceNumber);
}
