using SMOO.Server;

namespace SMOO.Services.Interface;

internal interface IRoomMessageProcessorList
{
    IRoomMessageProcessor GetProcessor(RoomMessageType type);
}
