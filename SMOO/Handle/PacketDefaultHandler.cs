using Microsoft.Extensions.Logging;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Impl;

namespace SMOO.Handle;

internal class PacketDefaultHandler : IPacketHandler
{
    public static ushort MinPayloadSize => 0;
    public static ushort MaxPayloadSize => Constants.MaxBufferSize;

    public static void Handle(RoomPacket packet, Room room, ServerContext context)
    {
        context.Logger.LogTrace("Default Packet Handler Involed for packet type: {PacketType}", packet.Header.Type);
    }
}
