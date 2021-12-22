using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using MessagePack;
using MessagePack.Formatters;

namespace FluentNest
{
    public class FnEventTimeFormatter : IMessagePackFormatter<object>
    {

        public void Serialize(ref MessagePackWriter writer, object value, MessagePackSerializerOptions options)
        {
            throw new System.NotImplementedException();
        }

        // https://github.com/neuecc/MessagePack-CSharp/blob/847c581d7a9ff98284eb609014627126777dacf5/src/MessagePack.UnityClient/Assets/Scripts/MessagePack/Formatters/TypelessFormatter.cs#L217
        public object Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            if (reader.NextMessagePackType == MessagePackType.Extension)
            {
                if (reader.NextCode is MessagePackCode.FixExt8 or MessagePackCode.Ext8)
                {
                    var peekReader = reader.CreatePeekReader();
                    var ext = peekReader.ReadExtensionFormatHeader();

                    if (ext.TypeCode == 0x00 && ext.Length == 8)
                    {
                        reader = peekReader;
                        var readOnlySequence = reader.ReadRaw(8);
                        // https://github.com/neuecc/MessagePack-CSharp/blob/ffc18319670d49246db1abbd05c404a820280776/src/MessagePack.UnityClient/Assets/Scripts/MessagePack/MessagePackReader.cs#L645
                        var seconds =
                            BinaryPrimitives.ReverseEndianness(
                                BitConverter.ToInt32(readOnlySequence.Slice(0, 4).ToArray()));
                        var nanoseconds =
                            BinaryPrimitives.ReverseEndianness(
                                BitConverter.ToInt32(readOnlySequence.Slice(4, 4).ToArray()));
                        return DateTimeConstants.UnixEpoch.AddSeconds(seconds)
                            .AddTicks(nanoseconds / DateTimeConstants.NanosecondsPerTick);
                    }
                }
            }

            return DynamicObjectTypeFallbackFormatter.Instance.Deserialize(ref reader, options);
        }

        
        // Copyright (c) All contributors. All rights reserved.
        // Licensed under the MIT license. See LICENSE file in the project root for full license information.

        /// <summary>
        /// Throws an exception indicating that there aren't enough bytes remaining in the buffer to store
        /// the promised data.
        /// </summary>
        private static EndOfStreamException ThrowNotEnoughBytesException() => throw new EndOfStreamException();

        /// <summary>
        /// Throws <see cref="EndOfStreamException"/> if a condition is false.
        /// </summary>
        /// <param name="condition">A boolean value.</param>
        /// <exception cref="EndOfStreamException">Thrown if <paramref name="condition"/> is <c>false</c>.</exception>
        private static void ThrowInsufficientBufferUnless(bool condition)
        {
            if (!condition)
            {
                ThrowNotEnoughBytesException();
            }
        }
    }

    // Copyright (c) All contributors. All rights reserved.
    // Licensed under the MIT license. See LICENSE file in the project root for full license information.

    internal static class DateTimeConstants
    {
        internal const long BclSecondsAtUnixEpoch = 62135596800;
        internal const int NanosecondsPerTick = 100;
        internal static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}