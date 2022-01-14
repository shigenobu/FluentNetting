using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.Pipelines;
using MessagePack;
using MessagePack.Formatters;

namespace FluentNetting.Formatters
{
    public sealed class FnCompressedEventStreamFormatter : IMessagePackFormatter<List<FnEntry>>
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

            using var memoryStream = new MemoryStream();
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
            {
                messagePackWriter.Flush();

                gZipStream.Write(arrayBufferWriter.WrittenSpan);
            }

            writer.Write(memoryStream.ToArray());
        }

        public List<FnEntry> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.NextMessagePackType != MessagePackType.Binary)
            {
                throw new MessagePackSerializationException("Invalid CompressedEventStream format.");
            }

            var readOnlySequence = reader.ReadBytes();
            var entries = new List<FnEntry>();
            if (readOnlySequence.HasValue)
            {
                using var memoryStream = new MemoryStream();
                using var stream = PipeReader
                    .Create((ReadOnlySequence<byte>) readOnlySequence).AsStream();
                using (var gZipStream = new GZipStream(stream, CompressionMode.Decompress))
                {
                    gZipStream.CopyTo(memoryStream);
                }

                var messagePackReader = new MessagePackReader(memoryStream.ToArray());
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