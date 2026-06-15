using System.Net;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Services.Interface;

internal interface IPacketSender
{
    Result<Error> Send(EndPoint destination, ReadOnlySpan<byte> buffer);
    void SendReliably(Player receiver, SharedBuffer buffer, Room room, byte maxRetries = Config.MaxRetries);
}
