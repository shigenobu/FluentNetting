using System.Buffers;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;

namespace FluentNest.Formatters
{
    public class FnEventStreamFormatter : IMessagePackFormatter<List<FnEntry>>
    {
        public void Serialize(ref MessagePackWriter writer, List<FnEntry> value, MessagePackSerializerOptions options)
        {
            // write bin
            var arrayBufferWriter = new ArrayBufferWriter<byte>();
            var messagePackWriter = writer.Clone(arrayBufferWriter);
            foreach (var entry in value)
            {
                MessagePackSerializer.Serialize(ref messagePackWriter, entry);
            }

            messagePackWriter.Flush();
            writer.Write(arrayBufferWriter.WrittenSpan);
        }

        public List<FnEntry> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.NextMessagePackType != MessagePackType.Binary &&
                reader.NextMessagePackType != MessagePackType.String)
            {
                throw new MessagePackSerializationException("Invalid EventStream format.");
            }

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
}