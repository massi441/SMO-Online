using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using Microsoft.Extensions.Logging;

namespace SMOO.Handle;

internal class PacketDisconnectHandler : IPacketHandler
{
    public static ushort MinPayloadSize => 0;
    public static ushort MaxPayloadSize => 0;

    public static void Handle(ParsedPacket packet, Room room, ServerContext context)
    {
        Player? player = packet.SenderPlayer;

        if (player == null)
        {
            context.Logger.LogWarning("Player was null in PacketDisconnect handler");
            return;
        }

        ServerResult disconnectResult = context.PlayerDisconnector.Disconnect(player);
        if (disconnectResult.IsFailed)
        {
            context.Logger.LogError("Unable to disconnect {PlayerName} in room #{RoomId}", player.Name, room.Id);
            return;
        }

        context.Logger.LogWarning("Player {Name} left room {RoomId}", player.Name, room.Id);
    }
}
