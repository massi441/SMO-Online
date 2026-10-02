using System.Diagnostics;
using Core.Util;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Impl;

namespace SMOO.Event;

internal readonly unsafe struct EventHandler
{
    public readonly ushort MinDataSize;
    public readonly ushort MaxDataSize;
    public readonly delegate*<EventPacket, Room, ServerContext, void> Handle;

    public EventHandler(ushort minPayloadSize, ushort maxPayloadSize, delegate*<EventPacket, Room, ServerContext, void> handle)
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
    private static readonly EventHandler GameSync               = MakeHandler<EventGameSyncHandler>();

    private static readonly EventHandler[] Handlers =
    [
        ChangeStage,
        ChangeCostume,
        ChangeCap,
        GameSync,
    ];

    static EventHandlerTable()
    {
        Debug.Assert(Handlers.Length == EnumUtil.GetEnumCount<EventType>(), "Handlers table is out of sync with EventType enum");
    }

    public static EventHandler GetHandler(EventType type)
    {
        ushort index = (ushort)type;

        return Handlers[index];
    }

    private static EventHandler MakeHandler<T>() where T : IEventHandler
    {
        return new EventHandler(T.MinDataSize, T.MaxDataSize, &T.Handle);
    }
}
