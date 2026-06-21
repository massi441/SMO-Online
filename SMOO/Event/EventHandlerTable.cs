using System.Diagnostics;
using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Event;

internal readonly unsafe struct EventHandler
{
    public readonly ushort MinDataSize;
    public readonly ushort MaxDataSize;
    public readonly delegate*<ParsedEventPacket, Room, ServerContext, void> Handle;

    public EventHandler(ushort minPayloadSize, ushort maxPayloadSize, delegate*<ParsedEventPacket, Room, ServerContext, void> handle)
    {
        MinDataSize = minPayloadSize;
        MaxDataSize = maxPayloadSize;
        Handle = handle;
    }
}

internal static unsafe class EventHandlerTable
{
    private static readonly EventHandler DefaultHandler         = MakeHandler<EventDefaultHandler>();
    private static readonly EventHandler ChangeStage            = MakeHandler<EventChangeStageHandler>();
    private static readonly EventHandler ChangeCostume          = MakeHandler<EventChangeCostumeHandler>();
    private static readonly EventHandler ChangeCap              = MakeHandler<EventChangeCapHandler>();
    private static readonly EventHandler PlayerSync             = MakeHandler<EventPlayerSyncHandler>();

    private static readonly EventHandler[] Handlers =
    [
        ChangeStage,
        ChangeCostume,
        ChangeCap,
        PlayerSync,
    ];

    static EventHandlerTable()
    {
        Debug.Assert(Handlers.Length == (ushort)EventType.OutOfRange, "Handlers table is out of sync with EventType enum");
    }

    public static EventHandler GetHandler(EventType type)
    {
        ushort index = (ushort)type;
        if (index < Handlers.Length)
        {
            return Handlers[index];
        }
        return DefaultHandler;
    }

    private static EventHandler MakeHandler<T>() where T : IEventHandler
    {
        return new EventHandler(T.MinDataSize, T.MaxDataSize, &T.Handle);
    }
}
