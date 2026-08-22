using SMOO.Server;
using SMOO.Services.Interface;

namespace SMOO.Services.Impl;

internal class RoomMessageProcessorList : IRoomMessageProcessorList
{
    private readonly IRoomMessageProcessor[] _services;

    public RoomMessageProcessorList(PacketMessageProcessor packetProcessor, PacketResendMessageProcessor packetResendProcessor, PlayerHealthMessageProcessor playerHealthProcessor)
    {
        int serviceCount = Enum.GetNames<RoomMessageType>().Length;

        _services = new IRoomMessageProcessor[serviceCount];

        this[RoomMessageType.Packet] = packetProcessor;
        this[RoomMessageType.PacketResend] = packetResendProcessor;
        this[RoomMessageType.PlayerHealthCheck] = playerHealthProcessor;
    }

    private IRoomMessageProcessor this[RoomMessageType type]
    {
        get
        {
            return _services[(byte)type];
        }
        set
        {
            _services[(byte)type] = value;
        }
    }

    public IRoomMessageProcessor GetProcessor(RoomMessageType type)
    {
        return this[type];
    }
}
