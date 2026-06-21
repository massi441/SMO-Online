using System.Diagnostics;
using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Handle;

internal readonly unsafe struct PacketHandler
{
    public readonly ushort MinPayloadSize;
    public readonly ushort MaxPayloadSize;
    public readonly delegate*<ParsedPacket, Room, ServerContext, void> Handler;

    public PacketHandler(ushort minPayloadSize, ushort maxPayloadSize, delegate*<ParsedPacket, Room, ServerContext, void> handler)
    {
        MinPayloadSize = minPayloadSize;
        MaxPayloadSize = maxPayloadSize;
        Handler = handler;
    }
}

internal static unsafe class PacketHandlerTable
{
    private static readonly PacketHandler DefaultHandler        = MakeHandler<PacketDefaultHandler>();
    private static readonly PacketHandler ConnectSyn            = MakeHandler<PacketConnectSynHandler>();
    private static readonly PacketHandler ConnectSynAck         = DefaultHandler;
    private static readonly PacketHandler ConnectAck            = MakeHandler<PacketConnectAckHandler>();
    private static readonly PacketHandler Disconnect            = MakeHandler<PacketDisconnectHandler>();
    private static readonly PacketHandler PlayerJoinRoom        = DefaultHandler;
    private static readonly PacketHandler HealthCheck           = MakeHandler<PacketHealthCheckHandler>();
    private static readonly PacketHandler Ping                  = DefaultHandler;
    private static readonly PacketHandler Ack                   = MakeHandler<PacketAckHandler>();
    private static readonly PacketHandler ChatMessage           = DefaultHandler;
    private static readonly PacketHandler ChatMessageRequest    = MakeHandler<PacketChatMessageHandler>();
    private static readonly PacketHandler Event                 = MakeHandler<PacketEventHandler>();
    private static readonly PacketHandler PlayersInStage        = DefaultHandler;

    private static readonly PacketHandler[] Handlers =
    [
        ConnectSyn,
        ConnectSynAck,
        ConnectAck,
        Disconnect,
        PlayerJoinRoom,
        HealthCheck,
        Ping,
        Ack,
        ChatMessage,
        ChatMessageRequest,
        Event,
        PlayersInStage
    ];

    static PacketHandlerTable()
    {
        Debug.Assert(Handlers.Length == (byte)PacketType.Invalid, "Handlers table is out of sync with PacketType enum");
    }

    public static PacketHandler GetHandler(PacketType type)
    {
        byte index = (byte)type;

        if (index < Handlers.Length)
        {
            return Handlers[index];
        }

        return DefaultHandler;
    }

    private static PacketHandler MakeHandler<T>() where T : IPacketHandler
    {
        return new PacketHandler(T.MinPayloadSize, T.MaxPayloadSize, &T.Handle);
    }
}
