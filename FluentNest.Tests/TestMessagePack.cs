using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using MessagePack;
using MessagePack.Resolvers;
using OrangeCabinet;
using Xunit;
using Xunit.Abstractions;

namespace FluentNest.Tests
{
    public class TestMessagePack
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TestMessagePack(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void TestForward()
        {
            var msg = new FnMessageForward();
            msg.Tag = "my.test";
            // msg.Entries = new List<FnMessageForwardEntry>();
            // msg.Entries.Add(new FnMessageForwardEntry()
            // {
            //     Time = 1234,
            //     Record = new Dictionary<string, object>()
            //     {
            //         {"k1", "v1"},
            //         {"k2", "v2"}
            //     }
            // });
            // msg.Entries.Add(new FnMessageForwardEntry()
            // {
            //     Time = 5678,
            //     Record = new Dictionary<string, object>()
            //     {
            //         {"k3", "v3"},
            //         {"k4", "v4"}
            //     }
            // });
            // msg.Option = new FnMessageForwardOption();
            // msg.Option.Chunk = "c";
            // msg.Option.Size = 12;
            // msg.Option.Compressed = "gzip";

            var data = MessagePackSerializer.Serialize(msg);
            _testOutputHelper.WriteLine(data.OxToString());
            var obj = MessagePackSerializer.Deserialize<FnMessageForward>(data);
            
            _testOutputHelper.WriteLine(MessagePackSerializer.SerializeToJson(obj));


            var entry = new FnMessageForwardEntry()
            {
                Time = new FnMessageForwardEntryTime
                {
                    UnixNanoseconds = 123,
                    // Ext = "ext"
                },
                Error = new FnMessageForwardEntryError()
                {
                    RetryTimes = 1,
                    Records = 2,
                    Error = "aaa",
                    Message = "bbb"
                }
            };
            _testOutputHelper.WriteLine(MessagePackSerializer.SerializeToJson(entry));
        }

        [Fact]
        public void TestFromFile()
        {
            var data = File.ReadAllBytes("/home/furuta/02-development/rider/FluentNest/FluentNest/data.bin");
            var obj = MessagePackSerializer.Deserialize<FnMessageForward>(data);

            _testOutputHelper.WriteLine(obj.Tag);
            // _testOutputHelper.WriteLine(obj.Entries.OxToString());

            // var entries = MessagePackSerializer.Deserialize<dynamic>(obj.Entries, ContractlessStandardResolver.Options);

            var entries = Deserialize<FnMessageForwardEntry>(obj.Entries);
            _testOutputHelper.WriteLine(MessagePackSerializer.SerializeToJson(entries));
            
            // var entries = MessagePackSerializer.Deserialize<FnMessageForwardEntry[]>(obj.Entries);
            // _testOutputHelper.WriteLine(MessagePackSerializer.SerializeToJson(entries));
            // if (obj.Option != null)
                // _testOutputHelper.WriteLine(obj.Option?.ToString());
            
            // _testOutputHelper.WriteLine(MessagePackSerializer.SerializeToJson(obj));
        }

        public List<T> Deserialize<T>(byte[] data)
        {
            var resolver = MessagePack.Resolvers.CompositeResolver.Create(
                new[] { MessagePack.Formatters.TypelessFormatter.Instance },
                new[] { MessagePack.Resolvers.StandardResolver.Instance });
            
            var list = new List<T>();
            using (var reader = new MessagePackStreamReader(new MemoryStream(data)))
            {
                while (reader.ReadAsync(CancellationToken.None).Result is { } msgpack)
                {
                    _testOutputHelper.WriteLine(MessagePackSerializer.ConvertToJson(msgpack));
                    list.Add((T)MessagePackSerializer.Typeless.Deserialize(msgpack));
                }
            }

            return list;
        }
    }
}