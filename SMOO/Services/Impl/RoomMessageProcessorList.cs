using SMOO.Server;
using SMOO.Services.Interface;

namespace SMOO.Services.Impl;

internal class RoomMessageProcessorList : IRoomMessageProcessorList
{
    private readonly IRoomMessageProcessor[] _services;

    public RoomMessageProcessorList(PacketProcessingService packetService, PacketResendService resendService, PlayerHealthCheckService healthCheckService)
    {
        int serviceCount = Enum.GetNames<RoomMessageType>().Length;

        _services = new IRoomMessageProcessor[serviceCount];

        this[RoomMessageType.Packet] = packetService;
        this[RoomMessageType.PacketResend] = resendService;
        this[RoomMessageType.PlayerHealthCheck] = healthCheckService;
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

    public IRoomMessageProcessor GetService(RoomMessageType type)
    {
        return this[type];
    }
}
