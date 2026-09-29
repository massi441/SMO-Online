using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Impl;

namespace SMOO.Event;

/// <summary>
/// An non virtual interface for defining an event handler
/// </summary>
internal interface IEventHandler
{
    /// <summary>
    /// The minimum data size for the event handled
    /// </summary>
    static abstract ushort MinDataSize { get; }

    /// <summary>
    /// The maximum data size for the event handled
    /// </summary>
    static abstract ushort MaxDataSize { get; }

    /// <summary>
    /// The handler for the event, called when the event is received
    /// </summary>
    /// <param name="packet">The event packet to handle</param>
    /// <param name="room">The room the event is from</param>
    /// <param name="context">The server context with services that may be required to handle the event</param>
    static abstract void Handle(EventPacket packet, Room room, ServerContext context);
}
