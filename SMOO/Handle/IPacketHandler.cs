using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Impl;

namespace SMOO.Handle;

/// <summary>
/// A non virtual interface for defining a packet handler
/// </summary>
internal interface IPacketHandler
{
    /// <summary>
    /// The minimum payload size for the packet handled
    /// </summary>
    static abstract ushort MinPayloadSize { get; }

    /// <summary>
    /// The maximum payload size for the packet handled
    /// </summary>
    static abstract ushort MaxPayloadSize { get; }

    /// <summary>
    /// The handler for the packet, called when a packet is received
    /// </summary>
    /// <param name="packet">The packet to handle</param>
    /// <param name="room">The room the packet is from</param>
    /// <param name="context">The server context with services that may be required to handle the packet</param>
    static abstract void Handle(RoomPacket packet, Room room, ServerContext context);
}
