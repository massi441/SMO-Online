using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using SMOO.Protocol;
using Core.Memory;

namespace SMOO.Server;

/// <summary>
/// A static utility class for packet-related operations.
/// </summary>
internal static class PacketUtil
{
    /// <summary>
    /// Writes the sequence number to the specified destination span at the correct offset for the PacketHeader structure.
    /// </summary>
    /// <param name="destination">The buffer to write the sequence number to.</param>
    /// <param name="sequenceNumber">The sequence number to write.</param>
    public static void WriteSequenceNumber(Span<byte> destination, ushort sequenceNumber)
    {
        SpanWriter writer = new SpanWriter(destination);

        int sequenceOffset = (int)Marshal.OffsetOf<PacketHeader>(nameof(PacketHeader.SequenceNumber));

        writer.Skip(sequenceOffset);
        writer.Write(sequenceNumber);
    }

    /// <summary>
    /// Sends an acknowledgment packet for the specified original packet and logs the result.
    /// Does not return the success or failure status.
    /// </summary>
    /// <param name="originalPacket">The original packet to acknowledge, containing the sequence number.</param>
    /// <param name="context">The server context.</param>
    public static void AckPacket(RoomPacket originalPacket, ServerContext context)
    {
        ServerResult ackResult = context.PacketController.SendAck(originalPacket);
        if (ackResult.IsSuccess)
        {
            context.Logger.LogTrace("Sent ack to {PlayerName}'s sequenced {PacketType} packet #{SequenceNumber}", originalPacket.SenderPlayer?.Name, originalPacket.Header.Type, originalPacket.Header.SequenceNumber);
        }
        else
        {
            context.Logger.LogError("Failed to ack {PlayerName}'s sequenced {PacketType} packet #{SequenceNumber}", originalPacket.SenderPlayer?.Name, originalPacket.Header.Type, originalPacket.Header.SequenceNumber);
        }
    }

    /// <summary>
    /// Sends an acknowledgment packet for the specified original event packet and logs the result.
    /// Does not return the success or failure status
    /// </summary>
    /// <param name="originalPacket">The original event packet to acknowledge.</param>
    /// <param name="context">The server context.</param>
    public static void AckEvent(EventPacket originalPacket, ServerContext context)
    {
        RoomPacket basePacket = originalPacket.BasePacket;

        ServerResult ackResult = context.PacketController.SendAck(basePacket);
        if (ackResult.IsSuccess)
        {
            context.Logger.LogTrace("Sent ack to {PlayerName}'s sequenced {PacketType} event packet #{SequenceNumber}", basePacket.SenderPlayer?.Name, originalPacket.EventHeader.Type, basePacket.Header.SequenceNumber);
        }
        else
        {
            context.Logger.LogError("Failed to ack {PlayerName}'s sequenced {PacketType} event packet #{SequenceNumber}", basePacket.SenderPlayer?.Name, originalPacket.EventHeader.Type, basePacket.Header.SequenceNumber);
        }
    }
}
