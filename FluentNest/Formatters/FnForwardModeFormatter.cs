using MessagePack;
using MessagePack.Formatters;

namespace FluentNest.Formatters
{
    public sealed class FnForwardModeFormatter : IMessagePackFormatter<FnForwardMode>
    {
        private static readonly FnEventModeFormatter EventModeFormatter = new();

        public void Serialize(ref MessagePackWriter writer, FnForwardMode value, MessagePackSerializerOptions options)
        {
            EventModeFormatter.Serialize(ref writer, value, options);
        }

        public FnForwardMode Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            return (FnForwardMode) EventModeFormatter.Deserialize(ref reader, options);
        }
    }
}