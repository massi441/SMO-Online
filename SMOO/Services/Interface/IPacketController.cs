using System.Net;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Memory;
using System.Net.Sockets;

namespace SMOO.Services.Interface;

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

    ServerResult SendAck(ParsedPacket originalPacket);

    /// <summary>
    /// Sends a packet
    /// </summary>
    void SendReliably(SharedBuffer buffer, Player receiver, Room room, byte maxRetries = Constants.MaxRetries, int resendDelay = Constants.DefaultResendDelay);

    ValueTask<SocketReceiveFromResult> ReceiveFromAsync(Memory<byte> buffer, SocketFlags flags, EndPoint remoteEndPoint, CancellationToken cancellationToken = default);
}
