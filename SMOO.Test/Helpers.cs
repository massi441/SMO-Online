using Core.Memory;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Interface;
using SMOO.Test.Stubs;

namespace SMOO.Test;

internal static class Helpers
{
    /// <summary>
    /// Advances the next sequence number of a store by uploading and removing packets, leaving the store empty
    /// </summary>
    /// <param name="store">The store to advance</param>
    /// <param name="player">The receiver of the uploaded packets</param>
    /// <param name="count">The amount of sequence numbers to advance by</param>
    public static void AdvanceStore(ISequencedPacketStore store, Player player, int count)
    {
        for (int i = 0; i < count; i++)
        {
            using RentedBuffer buffer = StubFactory.CreateBuffer();
            ServerResult<SequencedPacket> packet = store.UploadPacket(player, buffer);
            store.RemovePacket(packet.Data!.SequenceNumber);
        }
    }
}
