using System;
using System.Buffers;
using System.Buffers.Binary;
using MessagePack;
using MessagePack.Formatters;

namespace FluentNetting.Formatters
{
    public sealed class FnEventTimeFormatter : IMessagePackFormatter<DateTimeOffset>
    {
        public void Serialize(ref MessagePackWriter writer, DateTimeOffset value, MessagePackSerializerOptions options)
        {
            // Timestamp spec
            // https://github.com/fluent/fluentd/wiki/Forward-Protocol-Specification-v1#eventtime-ext-format
            // FixExt8(0) => seconds + nanoseconds | [1970-01-01 00:00:00.000000000 UTC, 2106-02-07T06:28:19.2949672 UTC) range
            // Reference implementation
            // https://github.com/neuecc/MessagePack-CSharp/blob/ffc18319670d49246db1abbd05c404a820280776/src/MessagePack.UnityClient/Assets/Scripts/MessagePack/MessagePackWriter.cs#L621
            var secondsSinceBclEpoch = value.UtcTicks / TimeSpan.TicksPerSecond;
            var seconds = secondsSinceBclEpoch - DateTimeConstants.BclSecondsAtUnixEpoch;
            var nanoseconds = (value.UtcTicks % TimeSpan.TicksPerSecond) * DateTimeConstants.NanosecondsPerTick;

            var data64 = unchecked((ulong) ((seconds << 32) | nanoseconds));
            Span<byte> span = writer.GetSpan(10);
            span[0] = MessagePackCode.FixExt8;
            span[1] = 0x00;
            WriteBigEndian(data64, span.Slice(2));
            writer.Advance(10);
        }

        private static void WriteBigEndian(ulong value, Span<byte> span)
        {
            unchecked
            {
                // Write to highest index first so the JIT skips bounds checks on subsequent writes.
                span[7] = (byte) value;
                span[6] = (byte) (value >> 8);
                span[5] = (byte) (value >> 16);
                span[4] = (byte) (value >> 24);
                span[3] = (byte) (value >> 32);
                span[2] = (byte) (value >> 40);
                span[1] = (byte) (value >> 48);
                span[0] = (byte) (value >> 56);
            }
        }

        public DateTimeOffset Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            // Reference implementation
            // https://github.com/neuecc/MessagePack-CSharp/blob/847c581d7a9ff98284eb609014627126777dacf5/src/MessagePack.UnityClient/Assets/Scripts/MessagePack/Formatters/TypelessFormatter.cs#L217
            if (reader.IsNil)
            {
                throw new MessagePackSerializationException("Data is Nil, FnEventTime can not be null.");
            }

            switch (reader.NextMessagePackType)
            {
                case MessagePackType.Integer:
                    return DateTimeOffset.UnixEpoch.AddSeconds(reader.ReadDouble());
                case MessagePackType.Extension:
                {
                    if (reader.NextCode is MessagePackCode.FixExt8 or MessagePackCode.Ext8)
                    {
                        var peekReader = reader.CreatePeekReader();
                        var ext = peekReader.ReadExtensionFormatHeader();

                        if (ext.TypeCode == 0x00 && ext.Length == 8)
                        {
                            reader = peekReader;
                            var readOnlySequence = reader.ReadRaw(8);
                            // Reference implementation
                            // https://github.com/neuecc/MessagePack-CSharp/blob/ffc18319670d49246db1abbd05c404a820280776/src/MessagePack.UnityClient/Assets/Scripts/MessagePack/MessagePackReader.cs#L645
                            var seconds =
                                BinaryPrimitives.ReverseEndianness(
                                    BitConverter.ToUInt32(readOnlySequence.Slice(0, 4).ToArray()));
                            var nanoseconds =
                                BinaryPrimitives.ReverseEndianness(
                                    BitConverter.ToUInt32(readOnlySequence.Slice(4, 4).ToArray()));

                            return DateTimeOffset.UnixEpoch.AddSeconds(seconds)
                                .AddTicks(nanoseconds / DateTimeConstants.NanosecondsPerTick);
                        }
                    }

                    break;
                }
            }

            throw new MessagePackSerializationException("Invalid EventTime format.");
        }
    }

    internal static class DateTimeConstants
    {
        internal const long BclSecondsAtUnixEpoch = 62135596800;
        internal const int NanosecondsPerTick = 100;
    }
}