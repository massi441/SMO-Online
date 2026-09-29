using Microsoft.Extensions.Logging;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Impl;

namespace SMOO.Event;

internal class EventDefaultHandler : IEventHandler
{
    public static ushort MinDataSize => 0;
    public static ushort MaxDataSize => Constants.MaxBufferSize;

    public static void Handle(EventPacket packet, Room room, ServerContext context)
    {
        context.Logger.LogTrace("Default Event Handler invoked for unhandled event in packet from {PlayerName}", packet.BasePacket.SenderPlayer?.Name);
    }
}
