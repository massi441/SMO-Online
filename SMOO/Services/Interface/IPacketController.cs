using System.Net;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using System.Net.Sockets;
using Core.Memory;

namespace SMOO.Services.Interface;

/// <summary>
/// Represents a controller for sending and receiving packets over the network
/// </summary>
internal interface IPacketController
{
    /// <summary>
    /// Sends a payload to any receiver
    /// </summary>
    ServerResult Send(ReadOnlySpan<byte> buffer, IPEndPoint receiver);

    /// <summary>
    /// Sends a payload to a player, and triggers a disconnection if the player's host is unreachable
    /// </summary>
    ServerResult Send(ReadOnlySpan<byte> buffer, Player receiver);

    /// <summary>
    /// Sends an acknowledgment packet to the sender of the original packet
    /// </summary>
    /// <param name="originalPacket"></param>
    /// <returns></returns>
    ServerResult SendAck(RoomPacket originalPacket);

    /// <summary>
    /// Sends a packet to a player and uploads it to the sequenced packet store for reliable delivery.
    /// </summary>
    void SendReliably(RentedBuffer buffer, Player receiver, SequencedPacketParams packetParams = default);

    /// <summary>
    /// Receives a packet from any sender, and returns the result of the receive operation, including the number of bytes received and the endpoint of the sender.
    /// </summary>
    ValueTask<SocketReceiveFromResult> ReceiveFromAsync(Memory<byte> buffer, SocketFlags flags, EndPoint remoteEndPoint, CancellationToken cancellationToken = default);
}
