using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using MessagePack;
using MsgPack;
using MsgPack.Serialization;
using Pigeon;
using Serilog;
using Serilog.Sinks.Fluentd;
using IMessagePackSerializer = FluentdClient.Sharp.IMessagePackSerializer;

namespace FluentNetting.Examples
{
    public class Program
    {
        public static void Main(string[] args)
        {
            FnLogger.Verbose = false;
            
            var server = new FnServer(new ExampleCallback())
            {
                Config = new FnConfig(),
                SettingClient = new FnSettingClient(),
                SettingServer = new FnSettingServer()
            };
            server.Start();

            // forward to server (and will be received)
            var timer = new Timer(3000);
            timer.Elapsed += async (sender, e) => await HandleTimer();
            timer.Start();

            server.WaitFor();
        }

        private static async Task HandleTimer()
        {
            // send to fluentd or fluent-bit in localhost

            // Pigeon (highly recommend)
            // https://www.nuget.org/packages/Pigeon/
            var config = new PigeonConfig("localhost", 24224);
            var clientPigeon = new PigeonClient(config);
            await clientPigeon.SendAsync(
                "tag.example2",
                new Dictionary<string, object> {["hello"] = "world2"}
            );
            
            // Serilog
            // https://www.nuget.org/packages/Serilog.Sinks.Fluentd/
            var options = new FluentdSinkOptions(
                "localhost", 24224, "tag.example1");
            var log = new LoggerConfiguration().WriteTo.Fluentd(options).CreateLogger();
            log.Information("hello {0}!", "world1");
            
            // FluentdClient.Sharp
            // https://www.nuget.org/packages/FluentdClient.Sharp/
            // (notice) CustomMessagePackSerializer is below.
            using (var client = new FluentdClient.Sharp.FluentdClient(
                       "localhost", 24224, new CustomMessagePackSerializer()))
            {
                await client.ConnectAsync();
                await client.SendAsync("tag.example3", 
                    new Dictionary<string, object> {["hello"] = "world3"});
            }
        }
    }

    public class ExampleCallback : IFnCallback
    {
        public void Receive(string tag, List<FnMessageEntry> entries)
        {
            Console.WriteLine($"tag:{tag}, entries:[{string.Join(", ", entries.Select(e => $"{{{e}}}"))}]");
        }
    }

    public class CustomMessagePackSerializer : IMessagePackSerializer
    {
        private static readonly IReadOnlyDictionary<Type, Func<object, MessagePackObject>> _typedMessagePackObjectFactories = new Dictionary<Type, Func<object, MessagePackObject>>
        {
            { typeof(short)         , obj => new MessagePackObject((short)obj) },
            { typeof(int)           , obj => new MessagePackObject((int)obj) },
            { typeof(long)          , obj => new MessagePackObject((long)obj) },
            { typeof(float)         , obj => new MessagePackObject((float)obj) },
            { typeof(double)        , obj => new MessagePackObject((double)obj) },
            { typeof(ushort)        , obj => new MessagePackObject((ushort)obj) },
            { typeof(uint)          , obj => new MessagePackObject((uint)obj) },
            { typeof(ulong)         , obj => new MessagePackObject((ulong)obj) },
            { typeof(string)        , obj => new MessagePackObject((string)obj) },
            { typeof(byte)          , obj => new MessagePackObject((byte)obj) },
            { typeof(sbyte)         , obj => new MessagePackObject((sbyte)obj) },
            { typeof(byte[])        , obj => new MessagePackObject((byte[])obj) },
            { typeof(bool)          , obj => new MessagePackObject((bool)obj) },
            { typeof(DateTime)      , obj => new MessagePackObject(MessagePackConvert.FromDateTime((DateTime)obj)) },
            { typeof(DateTimeOffset), obj => new MessagePackObject(MessagePackConvert.FromDateTimeOffset((DateTimeOffset)obj)) },
        };

        private readonly SerializationContext _context;

        public CustomMessagePackSerializer()
            : this(SerializationContext.Default)
        { }

        public CustomMessagePackSerializer(SerializationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public byte[] Serialize(string tag, string message)
        {
            return SerializeInternal(tag, message);
        }

        public byte[] Serialize<T>(string tag, T message) where T : class
        {
            return SerializeInternal(tag, message);
        }

        private byte[] SerializeInternal(string tag, object message)
        {
            var objects = new List<MessagePackObject>(3); // [ tag, timestamp, message ]

            objects.Add(tag);

            // timestamp must be converted to EventTime Ext Format or int64
            // case: EventTime Ext Format (means 100 nanosecond precision in .NET)
            var ticks = DateTimeOffset.Now.GetUnixTimestamp().Ticks;
            var seconds = ticks / TimeSpan.TicksPerSecond;
            var nanoseconds = (ticks % TimeSpan.TicksPerSecond) * 100;
            
            var data64 = unchecked((ulong) ((seconds << 32) | nanoseconds));
            var bytes = new []
            {
                (byte) (data64 >> 56),
                (byte) (data64 >> 48),
                (byte) (data64 >> 40),
                (byte) (data64 >> 32),
                (byte) (data64 >> 24),
                (byte) (data64 >> 16),
                (byte) (data64 >> 8),
                (byte) data64
            };

            objects.Add(new MessagePackObject(new MessagePackExtendedTypeObject(0x00, bytes)));

            // case: int64 (means second precision)
            // objects.Add(Convert.ToInt64(DateTimeOffset.Now.GetUnixTimestamp().TotalSeconds));
            
            objects.Add(CreateMessagePackObject(message));

            using (var stream = new MemoryStream())
            {
                var packer = Packer.Create(stream, PackerCompatibilityOptions.None);

                packer.Pack(new MessagePackObject(objects));

                return stream.ToArray();
            }
        }

        private MessagePackObject CreateMessagePackObject(object value)
        {
            var type = (value?.GetType() ?? typeof(string)).GetTypeInfo();

            if (_typedMessagePackObjectFactories.TryGetValue(type.AsType(), out var factory))
            {
                return factory.Invoke(value);
            }

            if (type.IsEnum)
            {
                return new MessagePackObject(value.ToString());
            }

            if (value is ExpandoObject || value is IDictionary<string, object>)
            {
                var obj = (IDictionary<string, object>)value;

                var dictionary = obj.ToDictionary(x => new MessagePackObject(x.Key), x => CreateMessagePackObject(x.Value));

                return new MessagePackObject(new MessagePackObjectDictionary(dictionary));
            }

            if (type.IsArray)
            {
                var obj = (object[])value;

                var array = obj.Select(x => CreateMessagePackObject(x)).ToArray();

                return new MessagePackObject(array);
            }

            if (type.GetInterfaces().Any(x => x == typeof(IEnumerable)))
            {
                var list = new List<MessagePackObject>();

                foreach (var item in (IEnumerable)value)
                {
                    list.Add(CreateMessagePackObject(item));
                }

                return new MessagePackObject(list);
            }

            if (type.IsAnonymous())
            {
                var properties = type.GetProperties(); // heavy

                var dictionary = properties.ToDictionary(x => new MessagePackObject(x.Name), x => CreateMessagePackObject(x.GetValue(value)));

                return new MessagePackObject(new MessagePackObjectDictionary(dictionary));
            }

            var objects = TypeAccessor.GetValueGetter(type.AsType())
                .ToDictionary(x => new MessagePackObject(x.Key), x => CreateMessagePackObject(x.Value.Invoke(value)));

            return new MessagePackObject(new MessagePackObjectDictionary(objects));
        }
    }

    internal static class Extensions
    {
        private static readonly DateTime _epochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTimeOffset _epochDateTimeOffset = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        internal static TimeSpan GetUnixTimestamp(this DateTime dateTime)
        {
            return dateTime.ToUniversalTime().Subtract(_epochDateTime);
        }

        internal static TimeSpan GetUnixTimestamp(this DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.ToUniversalTime().Subtract(_epochDateTimeOffset);
        }

        internal static bool IsAnonymous(this TypeInfo type)
        {
            var hasAttribute = type.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) != null;
            var containsName = type.FullName.Contains("AnonymousType");

            return hasAttribute && containsName;
        }
    }
    
    internal static class TypeAccessor
    {
        private static readonly ConcurrentDictionary<Type, IDictionary<string, Func<object, object>>> _valueGetter;

        static TypeAccessor()
        {
            _valueGetter = new ConcurrentDictionary<Type, IDictionary<string, Func<object, object>>>();
        }

        internal static IDictionary<string, object> GetMessageAsDictionary<T>(T message) where T : class
        {
            var type = message.GetType();

            var valueGetter = GetValueGetter(type);

            var dictionary = valueGetter.ToDictionary(x => x.Key, x => x.Value.Invoke(message));

            return dictionary;
        }

        internal static IDictionary<string, Func<object, object>> GetValueGetter(Type type)
        {
            return _valueGetter.GetOrAdd(
                type,
                valueType =>
                {
                    var properties = valueType.GetProperties();

                    return properties.ToDictionary(
                        x => x.Name,
                        x => new Func<object, object>(obj => x.GetValue(obj)));
                });
        }
    }
}