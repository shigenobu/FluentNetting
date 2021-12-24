using MessagePack;
using MessagePack.Formatters;

namespace FluentNest.Formatters
{
    public class FnPackedForwardModeFormatter : IMessagePackFormatter<FnPackedForwardMode>
    {
        private static readonly FnEventModeFormatter EventModeFormatter = new();

        public void Serialize(ref MessagePackWriter writer, FnPackedForwardMode value,
            MessagePackSerializerOptions options)
        {
            EventModeFormatter.Serialize(ref writer, value, options);
        }

        public FnPackedForwardMode Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            return (FnPackedForwardMode) EventModeFormatter.Deserialize(ref reader, options);
        }
    }
}