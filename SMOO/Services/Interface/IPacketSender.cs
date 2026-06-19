using System.Net;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Services.Interface;

internal interface IPacketSender
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
    void SendReliably(SharedBuffer buffer, Player receiver, IReliablePacketStore reliableStore, byte maxRetries = Config.MaxRetries, int resendDelay = Config.DefaultResendDelay);
}
