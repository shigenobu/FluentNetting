using System.Buffers;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;

namespace FluentNetting.Formatters;

public sealed class FnEventStreamFormatter : IMessagePackFormatter<List<FnEntry>>
{
    public void Serialize(ref MessagePackWriter writer, List<FnEntry> value, MessagePackSerializerOptions options)
    {
        // write bin
        // that is when to send other forward server, MessagePackEventStream is only as msgpack bin format.
        // see https://github.com/fluent/fluentd/wiki/Forward-Protocol-Specification-v1#packedforward-mode
        var arrayBufferWriter = new ArrayBufferWriter<byte>();
        var messagePackWriter = writer.Clone(arrayBufferWriter);
        foreach (var entry in value) MessagePackSerializer.Serialize(ref messagePackWriter, entry);

        messagePackWriter.Flush();
        writer.Write(arrayBufferWriter.WrittenSpan);
    }

    public List<FnEntry> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        // accept both formats of bin and str
        if (reader.NextMessagePackType != MessagePackType.Binary &&
            reader.NextMessagePackType != MessagePackType.String)
            throw new MessagePackSerializationException("Invalid EventStream format.");

        // repeatedly read as bin format and delegate deserialize
        var readOnlySequence = reader.ReadBytes();
        var entries = new List<FnEntry>();
        if (readOnlySequence.HasValue)
        {
            var messagePackReader = new MessagePackReader((ReadOnlySequence<byte>) readOnlySequence);
            while (!messagePackReader.End)
            {
                var entry = MessagePackSerializer.Deserialize<FnEntry>(messagePackReader.ReadRaw());
                entries.Add(entry);
            }
        }

        return entries;
    }
}