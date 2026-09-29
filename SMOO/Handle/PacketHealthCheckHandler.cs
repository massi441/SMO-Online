using Microsoft.Extensions.Logging;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Impl;

namespace SMOO.Handle;

internal class PacketHealthCheckHandler : IPacketHandler
{
    public static ushort MinPayloadSize => 0;
    public static ushort MaxPayloadSize => 0;

    public static void Handle(RoomPacket packet, Room room, ServerContext context)
    {
        context.Logger.LogTrace("Health check accepted");
    }
}
