using MessagePack;
using MessagePack.Formatters;

namespace FluentNest.Formatters
{
    public class FnCompressedPackedForwardFormatter : IMessagePackFormatter<FnCompressedPackedForwardMode>
    {
        private static readonly FnEventModeFormatter EventModeFormatter = new();

        public void Serialize(ref MessagePackWriter writer, FnCompressedPackedForwardMode value,
            MessagePackSerializerOptions options)
        {
            EventModeFormatter.Serialize(ref writer, value, options);
        }

        public FnCompressedPackedForwardMode Deserialize(ref MessagePackReader reader,
            MessagePackSerializerOptions options)
        {
            return (FnCompressedPackedForwardMode) EventModeFormatter.Deserialize(ref reader, options);
        }
    }
}