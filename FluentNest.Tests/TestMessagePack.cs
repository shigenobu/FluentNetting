using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using MessagePack;
using MessagePack.Resolvers;
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

            FnLogger.Verbose = true;
            FnLogger.Transfer = (msg) => _testOutputHelper.WriteLine(msg?.ToString());
        }

        [Fact]
        public void TestFowardError()
        {
            var data = File.ReadAllBytes("data/error.bin");

            FnMsgpackParser.TryParse(data, out var msg);
            _testOutputHelper.WriteLine(msg.ToString());
        }

        [Fact]
        public async void TestForwardError2()
        {
            var data = File.ReadAllBytes("data/error.bin");

            var msg = MessagePackSerializer.Deserialize<FnMsgpackInForwardMode>(data);

            var resolver = CompositeResolver.Create(
                FnEventTimeResolver.Instance,
                DynamicGenericResolver.Instance
            );

            // TODO: Include type info at FnMsgpackInForwardMode.Entries.(Should avoid FnMsgpackInForwardMode.Entries as byte[])
            var dataStructures = new List<object[]>();
            using (var streamReader = new MessagePackStreamReader(new MemoryStream(msg.Entries)))
            {
                while (await streamReader.ReadAsync(CancellationToken.None) is ReadOnlySequence<byte> msgpack)
                {
                    dataStructures.Add(MessagePackSerializer.Deserialize<object[]>(msgpack,
                        MessagePackSerializerOptions.Standard.WithResolver(resolver)));
                }
            }

            var count = 0;
            foreach (var dataStructure in dataStructures)
            {
                _testOutputHelper.WriteLine($"Entries[{count}]==========");
                count++;
                foreach (var o in dataStructure)
                {
                    if (o is Dictionary<object, object> d)
                    {
                        foreach (var keyValuePair in d)
                        {
                            _testOutputHelper.WriteLine(keyValuePair.ToString());
                        }
                    }
                    else
                    {
                        _testOutputHelper.WriteLine($"EventTime: {o.ToString()}");
                    }
                }
            }
        }
    }
}