using SMOO.Server;

namespace SMOO.Services.Interface;

internal interface IRoomMessageProcessorList
{
    IRoomMessageProcessor GetService(RoomMessageType type);
}
