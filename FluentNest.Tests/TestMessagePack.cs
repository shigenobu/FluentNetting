using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
                // Time = new FnMessageForwardEntryTime
                // {
                //     UnixNanoseconds = 123,
                //     // Ext = "ext"
                // },
                // Error = new FnMessageForwardEntryError()
                // {
                //     RetryTimes = 1,
                //     Records = 2,
                //     Error = "aaa",
                //     Message = "bbb"
                // }
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

            StringBuilder builder = new StringBuilder();
            foreach (var b in obj.Entries)
            {
                builder.Append(b.ToString("X") + " ");
            }
            // _testOutputHelper.WriteLine(builder.ToString());
            
            // var entries = MessagePackSerializer.Deserialize<dynamic>(obj.Entries, ContractlessStandardResolver.Options);

            var entries = Deserialize<FnMessageForwardEntry>(obj.Entries);
            foreach (var entry in entries)
            {
                _testOutputHelper.WriteLine(entry.ToString());
            }

            
            // var entries = MessagePackSerializer.Deserialize<FnMessageForwardEntry[]>(obj.Entries);
            // _testOutputHelper.WriteLine(MessagePackSerializer.SerializeToJson(entries));
            // if (obj.Option != null)
                // _testOutputHelper.WriteLine(obj.Option?.ToString());
            
            // _testOutputHelper.WriteLine(MessagePackSerializer.SerializeToJson(obj));
        }

        public List<T> Deserialize<T>(byte[] data)
        {
            byte[] replaced = {0xd2, 0x00, 0x00, 0x00, 0x00};
            
            var resolver = MessagePack.Resolvers.CompositeResolver.Create(
                new[] { MessagePack.Formatters.TypelessFormatter.Instance },
                new[] { MessagePack.Resolvers.StandardResolver.Instance });
            var list = new List<T>();
            using (var reader = new MessagePackStreamReader(new MemoryStream(data)))
            {
                while (reader.ReadAsync(CancellationToken.None).Result is { } msgpack)
                {
                    // ReservedMessagePackExtensionTypeCode.

                    // MessagePackReader r;
                    //
                    StringBuilder builder = new StringBuilder();
                    foreach (var b in msgpack.ToArray())
                    {
                        builder.Append(b.ToString("X") + " ");
                    }
                    _testOutputHelper.WriteLine(builder.ToString());
                    _testOutputHelper.WriteLine(msgpack.ToArray().OxToString());
                    //
                    var newBytes = Concat(Concat(msgpack.Slice(0, 1).ToArray(), replaced), msgpack.Slice(11).ToArray());
                    StringBuilder builder2 = new StringBuilder();
                    foreach (var b in newBytes)
                    {
                        builder2.Append(b.ToString("X") + " ");
                    }
                    _testOutputHelper.WriteLine(builder2.ToString());
                    _testOutputHelper.WriteLine(newBytes.OxToString());
                    
                    // _testOutputHelper.WriteLine(MessagePackSerializer.ConvertToJson(msgpack));
                    list.Add(MessagePackSerializer.Deserialize<T>(newBytes));
                }
            }

            return list;
        }
        
        static byte[] Concat(byte[] a, byte[] b)
        {           
            byte[] output = new byte[a.Length + b.Length];
            for (int i = 0; i < a.Length; i++)
                output[i] = a[i];
            for (int j = 0; j < b.Length; j++)
                output[a.Length+j] = b[j];
            return output;           
        }
    }
}