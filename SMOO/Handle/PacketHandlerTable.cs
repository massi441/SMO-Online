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
    private static readonly PacketHandler DefaultHandler        = new PacketHandler(PacketDefaultHandler.MinPayloadSize,        PacketDefaultHandler.MaxPayloadSize,        &PacketDefaultHandler.Handle);
    private static readonly PacketHandler ConnectSyn            = new PacketHandler(PacketConnectSynHandler.MinPayloadSize,         PacketConnectSynHandler.MaxPayloadSize,         &PacketConnectSynHandler.Handle);
    private static readonly PacketHandler ConnectSynAck         = DefaultHandler;
    private static readonly PacketHandler ConnectAck            = new PacketHandler(PacketConnectAckHandler.MinPayloadSize, PacketConnectAckHandler.MaxPayloadSize, &PacketConnectAckHandler.Handle);
    private static readonly PacketHandler Disconnect            = new PacketHandler(PacketDisconnectHandler.MinPayloadSize,      PacketDisconnectHandler.MaxPayloadSize,      &PacketDisconnectHandler.Handle);
    private static readonly PacketHandler PlayerJoinRoom        = DefaultHandler;
    private static readonly PacketHandler HealthCheck           = new PacketHandler(PacketHealthCheckHandler.MinPayloadSize,     PacketHealthCheckHandler.MaxPayloadSize,     &PacketHealthCheckHandler.Handle);
    private static readonly PacketHandler Ping                  = DefaultHandler;
    private static readonly PacketHandler Ack                   = new PacketHandler(PacketAckHandler.MinPayloadSize,             PacketAckHandler.MaxPayloadSize,             &PacketAckHandler.Handle);
    private static readonly PacketHandler ChatMessage           = DefaultHandler;
    private static readonly PacketHandler ChatMessageRequest    = new PacketHandler(PacketChatMessageHandler.MinPayloadSize,     PacketChatMessageHandler.MaxPayloadSize,     &PacketChatMessageHandler.Handle);
    private static readonly PacketHandler Event                 = new PacketHandler(PacketEventHandler.MinPayloadSize, PacketEventHandler.MaxPayloadSize, &PacketEventHandler.Handle);
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
}
