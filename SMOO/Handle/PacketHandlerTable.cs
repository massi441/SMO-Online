using System.Diagnostics;
using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Handle;

/// <summary>
/// Represents an SMOO packet handler. Wraps a Minimum and Maximum payload size for data integrity,
/// and a function pointer to the appropriate handler function.
/// </summary>
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

/// <summary>
/// Represents a packet handler table, where each slot in the table maps to the handler of a given PacketType.
/// </summary>
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
        Debug.Assert(Handlers.Length == (byte)PacketType.OutOfRange, "Handlers table is out of sync with PacketType enum");
    }

    public static PacketHandler GetHandler(PacketType type)
    {
        byte index = (byte)type;

        return Handlers[index];
    }

    /// <summary>
    /// Creates a PacketHandlerTable handler entry out of an IPacketHandler type.
    /// Wraps its size bounds and stores a pointer to the handle function
    /// </summary>
    /// <typeparam name="T">Type type of packet handler</typeparam>
    /// <returns>A PacketHandler for the PacketHandlerTable</returns>
    private static PacketHandler MakeHandler<T>() where T : IPacketHandler
    {
        return new PacketHandler(T.MinPayloadSize, T.MaxPayloadSize, &T.Handle);
    }
}
