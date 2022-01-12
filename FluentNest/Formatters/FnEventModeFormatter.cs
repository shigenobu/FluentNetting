using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.Pipelines;
using MessagePack;
using MessagePack.Formatters;

namespace FluentNest.Formatters
{
    public sealed class FnEventModeFormatter : IMessagePackFormatter<BaseFnEventMode>
    {
        private static readonly FnEventTimeFormatter EventTimeFormatter = new();

        private static readonly FnEventStreamFormatter EventStreamFormatter = new();

        private static readonly FnCompressedEventStreamFormatter CompressedEventStreamFormatter = new();

        public void Serialize(ref MessagePackWriter writer, BaseFnEventMode value, MessagePackSerializerOptions options)
        {
            // detect object type and ignore optional content is null or empty value
            // because of [Key(n)] attributes could not ignore null or empty value
            // i.e. [Key(n)] attributes serialize data to fixed length array and length is number of members
            switch (value)
            {
                case FnMessageMode o:
                    var hasMessageModeOption = o.Option is not null && o.Option.Count != 0;
                    if (hasMessageModeOption)
                    {
                        writer.WriteArrayHeader(4);
                    }
                    else
                    {
                        writer.WriteArrayHeader(3);
                    }

                    writer.Write(o.Tag);

                    EventTimeFormatter.Serialize(ref writer, o.EventTime, options);

                    MessagePackSerializer.Serialize(ref writer, o.Record);

                    if (hasMessageModeOption)
                    {
                        MessagePackSerializer.Serialize(ref writer, o.Option!);
                    }

                    return;
                case FnForwardMode o:
                    var hasForwardModeOption = o.Option is not null && o.Option.Count != 0;
                    if (hasForwardModeOption)
                    {
                        writer.WriteArrayHeader(3);
                    }
                    else
                    {
                        writer.WriteArrayHeader(2);
                    }

                    writer.Write(o.Tag);

                    MessagePackSerializer.Serialize(ref writer, o.Entries);

                    if (hasForwardModeOption)
                    {
                        MessagePackSerializer.Serialize(ref writer, o.Option!);
                    }

                    return;
                case FnPackedForwardMode o:
                    var hasPackedForwardModeOption = o.Option is not null && o.Option.Count != 0;
                    if (hasPackedForwardModeOption)
                    {
                        writer.WriteArrayHeader(3);
                    }
                    else
                    {
                        writer.WriteArrayHeader(2);
                    }

                    writer.Write(o.Tag);

                    EventStreamFormatter.Serialize(ref writer, o.Entries, options);

                    if (hasPackedForwardModeOption)
                    {
                        MessagePackSerializer.Serialize(ref writer, o.Option!);
                    }

                    return;
                case FnCompressedPackedForwardMode o:
                    // TODO: check
                    writer.WriteArrayHeader(3);
                    writer.Write(o.Tag);

                    CompressedEventStreamFormatter.Serialize(ref writer, o.Entries, options);

                    o.Option["compressed"] = "gzip";
                    MessagePackSerializer.Serialize(ref writer, o.Option);
                    return;
            }

            throw new MessagePackSerializationException("Invalid type.");
        }

        public BaseFnEventMode Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.IsNil)
            {
                throw new MessagePackSerializationException("Data is Nil, FnEventMode can not be null.");
            }

            // To detect mode at first.
            var peekReader = reader.CreatePeekReader();

            var count = peekReader.ReadArrayHeader();
            if (count is < 2 or > 4)
            {
                throw new MessagePackSerializationException("Invalid BaseFnEventMode count.");
            }

            // tag must be first element
            var tag = peekReader.ReadString();
            if (tag == null)
            {
                throw new MessagePackSerializationException("1st element is Nil, 1st element can not be null.");
            }

            switch (peekReader.NextMessagePackType)
            {
                case MessagePackType.Extension or MessagePackType.Integer:
                    // Message Modes
                    return MessagePackSerializer.Deserialize<FnMessageMode>(ref reader);
                case MessagePackType.Array:
                    // Forward Mode
                    return MessagePackSerializer.Deserialize<FnForwardMode>(ref reader);
                case MessagePackType.String:
                    // Comment out why fluentd compressed mode sends string format.
                    // return MessagePackSerializer.Deserialize<FnPackedForwardMode>(ref reader);
                case MessagePackType.Binary:
                    reader = peekReader;
                    // PackedForward Mode or CompressedPackedForward Mode
                    if (count == 4)
                    {
                        throw new MessagePackSerializationException(
                            "Invalid FnPackedForwardMode/FnCompressedPackedForwardMode count.");
                    }

                    BaseFnEventMode result = null;
                    var readOnlySequence = reader.ReadBytes();
                    if (count == 3)
                    {
                        // PackedForward Mode or CompressedPackedForward Mode
                        if (reader.NextMessagePackType == MessagePackType.Map)
                        {
                            var option = MessagePackSerializer.Deserialize<Dictionary<string, object>>(ref reader);

                            if (option is not null && option.TryGetValue("compressed", out var type))
                            {
                                if (type is string value)
                                {
                                    if (value == "gzip")
                                    {
                                        // CompressedPackedForward Mode
                                        result = new FnCompressedPackedForwardMode { Option = option };
                                        // decompress and replace compressed data to decompressed data
                                        // deserialization is deferred
                                        if (readOnlySequence.HasValue)
                                        {
                                            using var memoryStream = new MemoryStream();
                                            using var stream = PipeReader
                                                .Create((ReadOnlySequence<byte>) readOnlySequence).AsStream();
                                            using (var gZipStream = new GZipStream(stream, CompressionMode.Decompress))
                                            {
                                                gZipStream.CopyTo(memoryStream);
                                            }

                                            readOnlySequence = new ReadOnlySequence<byte>(memoryStream.ToArray());
                                        }
                                    }
                                    // else
                                    // {
                                    //     throw new MessagePackSerializationException(
                                    //         "option.compressed must be 'gzip'.");
                                    // }
                                }
                                else
                                {
                                    throw new MessagePackSerializationException("option.compressed must be string.");
                                }
                            }

                            result ??= new FnPackedForwardMode { Option = option }; // PackedForward Mode(have option)
                        }
                        else
                        {
                            throw new MessagePackSerializationException(
                                "3rd element's type is not map, 3rd element's type can only be map.");
                        }
                    }

                    result ??= new FnPackedForwardMode(); // PackedForward Mode(no option)

                    result.Tag = tag;

                    // deserialize entries(that is stream data)
                    if (readOnlySequence.HasValue)
                    {
                        var messagePackReader = new MessagePackReader((ReadOnlySequence<byte>) readOnlySequence);
                        var entries = new List<FnEntry>();
                        while (!messagePackReader.End)
                        {
                            var entry = MessagePackSerializer.Deserialize<FnEntry>(messagePackReader.ReadRaw());
                            entries.Add(entry);
                        }

                        switch (result)
                        {
                            case FnPackedForwardMode o:
                                o.Entries = entries;
                                break;
                            case FnCompressedPackedForwardMode o:
                                o.Entries = entries;
                                break;
                        }
                    }

                    return result;
                default:
                    throw new MessagePackSerializationException("Invalid type at 2nd element.");
            }
        }
    }
}