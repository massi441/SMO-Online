using Core.Memory;
using SMOO.Client;
using SMOO.Protocol;

namespace SMOO.Services.Interface;

internal interface ISequencedPacketStore
{
    SequencedPacket UploadPacket(Player receiver, RentedBuffer buffer, SequencedPacketParams packetParams = default);
    SequencedPacket? RemovePacket(ushort sequenceNumber);
    void ResendPackets();
    void Clear();
}
