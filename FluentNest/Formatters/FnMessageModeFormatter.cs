using MessagePack;
using MessagePack.Formatters;

namespace FluentNest.Formatters
{
    public class FnMessageModeFormatter : IMessagePackFormatter<FnMessageMode>
    {
        private static readonly FnEventModeFormatter EventModeFormatter = new();

        public void Serialize(ref MessagePackWriter writer, FnMessageMode value, MessagePackSerializerOptions options)
        {
            EventModeFormatter.Serialize(ref writer, value, options);
        }

        public FnMessageMode Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            return (FnMessageMode) EventModeFormatter.Deserialize(ref reader, options);
        }
    }
}