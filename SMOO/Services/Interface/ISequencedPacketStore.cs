using Core.Memory;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Services.Interface;

/// <summary>
/// Represents a store for sequenced packets, allowing for reliable delivery of packets to players
/// </summary>
internal interface ISequencedPacketStore
{
    /// <summary>
    /// Uploads a packet to the store for a specific receiver, with optional parameters for sequencing and retries.
    /// Rejects the packet if the store is full
    /// </summary>
    /// <param name="receiver">The player to whom the packet will be sent</param>
    /// <param name="buffer">The buffer containing the packet data</param>
    /// <param name="packetParams">The configuration of the sequenced packet</param>
    /// <returns>The sequenced packet that was uploaded, or null if the upload failed</returns>
    ServerResult<SequencedPacket> UploadPacket(Player receiver, RentedBuffer buffer, SequencedPacketParams packetParams = default);

    /// <summary>
    /// Removes a packet from the store based on its sequence number, if it exists in the current store
    /// </summary>
    /// <param name="sequenceNumber">The sequence number of the packet to remove</param>
    /// <returns>The removed packet, or null if no packet with the specified sequence number exists</returns>
    SequencedPacket? RemovePacket(ushort sequenceNumber);

    /// <summary>
    /// Looks over all packets in the current store and resends any packets that have not been acknowledged within the configured timeout period.
    /// </summary>
    void ResendPackets();

    /// <summary>
    /// Clears all packets in the store and ensures their <see cref="RentedBuffer"/> references are released.
    /// </summary>
    /// <returns>The number of packets that were cleared</returns>
    int Clear();
}
