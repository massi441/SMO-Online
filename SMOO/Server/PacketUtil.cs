using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using SMOO.Protocol;
using SMOO.Memory;

namespace SMOO.Server;

internal static class PacketUtil
{
    public static void WriteSequenceNumber(Span<byte> destination, ushort sequenceNumber)
    {
        SpanWriter writer = new SpanWriter(destination);

        int sequenceOffset = (int)Marshal.OffsetOf<PacketHeader>(nameof(PacketHeader.SequenceNumber));

        writer.Skip(sequenceOffset);
        writer.Write(sequenceNumber);
    }

    public static void AckPacket(ParsedPacket originalPacket, ServerContext context)
    {
        ServerResult ackResult = context.PacketSender.SendAck(originalPacket);
        if (ackResult.IsSuccess)
        {
            context.Logger.LogTrace("Sent ack to {PlayerName}'s reliable {PacketType} packet #{SequenceNumber}", originalPacket.SenderPlayer?.Name, originalPacket.Header.Type, originalPacket.Header.SequenceNumber);
        }
        else
        {
            context.Logger.LogError("Failed to ack {PlayerName}'s reliable {PacketType} packet #{SequenceNumber}", originalPacket.SenderPlayer?.Name, originalPacket.Header.Type, originalPacket.Header.SequenceNumber);
        }
    }

    public static void AckEvent(ParsedEventPacket originalPacket, ServerContext context)
    {
        ParsedPacket basePacket = originalPacket.BasePacket;

        ServerResult ackResult = context.PacketSender.SendAck(basePacket);
        if (ackResult.IsSuccess)
        {
            context.Logger.LogTrace("Sent ack to {PlayerName}'s reliable {PacketType} event packet #{SequenceNumber}", basePacket.SenderPlayer?.Name, originalPacket.EventHeader.Type, basePacket.Header.SequenceNumber);
        }
        else
        {
            context.Logger.LogError("Failed to ack {PlayerName}'s reliable {PacketType} event packet #{SequenceNumber}", basePacket.SenderPlayer?.Name, originalPacket.EventHeader.Type, basePacket.Header.SequenceNumber);
        }
    }
}
