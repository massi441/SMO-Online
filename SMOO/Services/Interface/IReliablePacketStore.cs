using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Threading;
using SMOO.Memory;

namespace SMOO.Services.Interface;

internal interface IReliablePacketStore
{
    public LockedDictionary<ushort, ReliablePacket> PendingPackets { get; }

    public ReliablePacket UploadPacket(SharedBuffer buffer, Player receiver, byte maxRetries = Constants.MaxRetries, int resendDelay = Constants.DefaultResendDelay);

    public void ClearPlayer(Player player);

    /// <summary>
    /// Removes a reliable packet, and returns its rented buffer to the array pool.
    /// </summary>
    /// <param name="sequenceNumber">The sequence number of the packet to remove</param>
    /// <returns>The removed packed</returns>
    public ReliablePacket? RemovePacket(Player requester, ushort sequenceNumber);
}
