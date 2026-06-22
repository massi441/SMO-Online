using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using SMOO.Event;
using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Handle;

internal class PacketEventHandler : IPacketHandler
{
    public static ushort MinPayloadSize => (ushort)Unsafe.SizeOf<EventHeader>();
    public static ushort MaxPayloadSize => Constants.MaxBufferSize;

    public static void Handle(ParsedPacket packet, Room room, ServerContext context)
    {
        ParsedEventPacket eventPacket = new ParsedEventPacket() { BasePacket = packet };

        EventType eventType = eventPacket.EventHeader.Type;

        //context.Logger.LogTrace("Dispatching event {EventType} from {PlayerName}", eventType, packet.SenderPlayer?.Name); // TODO: add verbose level

        Event.EventHandler handler = EventHandlerTable.GetHandler(eventType);

        if (eventPacket.EventData.Length < handler.MinDataSize)
        {
            context.Logger.LogWarning("Event {EventType} data too small ({Size}), minimum required: {Minimum}", eventType, eventPacket.EventData.Length, handler.MinDataSize);
            return;
        }

        if (eventPacket.EventData.Length > handler.MaxDataSize)
        {
            context.Logger.LogWarning("Event {EventType} data too large ({Size}), maximum allowed: {Maximum}. Error: {Error}", eventType, eventPacket.EventData.Length, handler.MaxDataSize, ServerError.PayloadTooLarge);
            return;
        }

        eventPacket.EventHeader.PlayerSlot = packet.SenderPlayer!.Slot;

        unsafe
        {
            handler.Handle(eventPacket, room, context);
        }
    }
}
