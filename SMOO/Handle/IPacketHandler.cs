using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Impl;

namespace SMOO.Handle;

internal interface IPacketHandler
{
    static abstract ushort MinPayloadSize { get; }
    static abstract ushort MaxPayloadSize { get; }
    //static abstract bool RequiresPlayer { get; } // TODO: uncomment when ready
    static abstract void Handle(ParsedPacket packet, Room room, ServerContext context);
}
