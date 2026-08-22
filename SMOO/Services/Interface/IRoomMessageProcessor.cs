using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Services.Interface;

internal interface IRoomMessageProcessor
{
    void Process(Room room, Packet packet);
}
