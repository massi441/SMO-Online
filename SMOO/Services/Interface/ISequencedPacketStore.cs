using Core.Memory;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Services.Interface;

internal interface ISequencedPacketStore
{
    ServerResult<SequencedPacket> UploadPacket(Player receiver, RentedBuffer buffer, SequencedPacketParams packetParams = default);
    SequencedPacket? RemovePacket(ushort sequenceNumber);
    void ResendPackets();
    int Clear();
}
