using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using Microsoft.Extensions.Logging;
using SMOO.Services.Impl;

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

        room.RequestDisconnection(player);

        context.Logger.LogWarning("Player {Name} left room {RoomId} and will be disconnected shortly", player.Name, room.Id);
    }
}
