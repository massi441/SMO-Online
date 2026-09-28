using SMOO.Client;
using SMOO.Server;

namespace SMOO.Services.Interface;

internal interface IRoom
{
    int Id { get; }
    IPlayerHolder PlayerHolder { get; }
    IBroadcaster Broadcaster { get; }
    PlayerList Players => PlayerHolder.Players;

    void Start();
    Task Shutdown();
    void UploadMessage(RoomMessage roomMessage);
    void RequestDisconnection(Player player);
}
