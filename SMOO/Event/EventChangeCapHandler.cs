using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Event;

internal class EventChangeCapHandler : IEventHandler
{
    public static ushort MinDataSize => throw new NotImplementedException();

    public static ushort MaxDataSize => throw new NotImplementedException();

    public static void Handle(ParsedEventPacket packet, Room room, ServerContext context)
    {
        throw new NotImplementedException();
    }
}
