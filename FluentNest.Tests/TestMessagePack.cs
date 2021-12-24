using System;
using System.Collections.Generic;
using System.IO;
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
        public void TestForwardError2()
        {
            var data = File.ReadAllBytes("data/error.bin");

            var resolver = CompositeResolver.Create(
                FnEventModeResolver.Instance,
                StandardResolver.Instance
            );

            var msg = MessagePackSerializer.Deserialize<BaseFnEventMode>(data,
                MessagePackSerializerOptions.Standard.WithResolver(resolver));
            switch (msg)
            {
                case FnMessageMode o:
                    break;
                case FnForwardMode o:
                    break;
                case FnPackedForwardMode o:
                    FnLogger.Info(o.Tag);
                    foreach (var entry in o.Entries)
                    {
                        foreach (var (key, value) in entry.Record)
                        {
                            FnLogger.Info($"{entry.EventTime} {key}: {value}");
                        }
                    }

                    if (o.Option != null)
                    {
                        foreach (var (key, value) in o.Option)
                        {
                            FnLogger.Info($"{key}: {value}");
                        }
                    }

                    break;
                case FnCompressedPackedForwardMode o:
                    break;
            }

            var bin = MessagePackSerializer.Deserialize<FnPackedForwardMode>(data);
            var serialize = MessagePackSerializer.Serialize(bin);
            FnLogger.Info(BitConverter.ToString(serialize).Replace("-", " "));
        }

        [Fact]
        public void TestMessageMode()
        {
            var mode = new FnMessageMode()
            {
                Tag = "tag.name",
                EventTime = new DateTime(2020, 1, 2, 3, 4, 5, 123).AddTicks(4567),
                Record = new Dictionary<string, object> { { "message", "foo" } }
            };

            var resolver = CompositeResolver.Create(
                FnEventModeResolver.Instance,
                StandardResolver.Instance
            );
            var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
            var serialize = MessagePackSerializer.Serialize(mode, options);
            var msg = BitConverter.ToString(serialize).Replace("-", " ");
            FnLogger.Info(msg);
            var deserialize = MessagePackSerializer.Deserialize<FnMessageMode>(serialize, options);
            FnLogger.Info(deserialize.Tag);
            FnLogger.Info(deserialize.EventTime.ToString("O"));
            FnLogger.Info(deserialize.Record["message"]);
            FnLogger.Info(deserialize.Option);
            var deserialize2 = (FnMessageMode) MessagePackSerializer.Deserialize<BaseFnEventMode>(serialize, options);
            FnLogger.Info(deserialize2.Tag);
            FnLogger.Info(deserialize2.EventTime.ToString("O"));
            FnLogger.Info(deserialize2.Record["message"]);
            FnLogger.Info(deserialize2.Option);
        }

        [Fact]
        public void TestForwardMode()
        {
            var mode = new FnForwardMode()
            {
                Tag = "tag.name",
            };
            var entries = new List<FnEntry>();
            var entry = new FnEntry
            {
                EventTime = new DateTime(2020, 1, 2, 3, 4, 5, 123).AddTicks(4567),
                Record = new Dictionary<string, object> { { "message", "foo" } }
            };
            entries.Add(entry);
            mode.Entries = entries;

            var resolver = CompositeResolver.Create(
                FnEventModeResolver.Instance,
                StandardResolver.Instance
            );
            var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
            var serialize = MessagePackSerializer.Serialize(mode, options);
            var msg = BitConverter.ToString(serialize).Replace("-", " ");
            FnLogger.Info(msg);
            var deserialize = MessagePackSerializer.Deserialize<FnForwardMode>(serialize, options);
            FnLogger.Info(deserialize.Tag);
            foreach (var e in deserialize.Entries)
            {
                FnLogger.Info(e.EventTime.ToString("O"));
                FnLogger.Info(e.Record["message"]);
            }

            FnLogger.Info(deserialize.Option);
            var deserialize2 = (FnForwardMode) MessagePackSerializer.Deserialize<BaseFnEventMode>(serialize, options);
            FnLogger.Info(deserialize2.Tag);
            foreach (var e in deserialize2.Entries)
            {
                FnLogger.Info(e.EventTime.ToString("O"));
                FnLogger.Info(e.Record["message"]);
            }

            FnLogger.Info(deserialize2.Option);
        }

        [Fact]
        public void TestPackedForwardMode()
        {
            var mode = new FnPackedForwardMode
            {
                Tag = "tag.name"
            };
            var entries = new List<FnEntry>();
            var entry = new FnEntry
            {
                EventTime = new DateTime(2020, 1, 2, 3, 4, 5, 123).AddTicks(4567),
                Record = new Dictionary<string, object> { { "message", "foo" } }
            };
            entries.Add(entry);
            mode.Entries = entries;

            var resolver = CompositeResolver.Create(
                FnEventModeResolver.Instance,
                StandardResolver.Instance
            );
            var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
            var serialize = MessagePackSerializer.Serialize(mode, options);
            var msg = BitConverter.ToString(serialize).Replace("-", " ");
            FnLogger.Info(msg);
            var deserialize = MessagePackSerializer.Deserialize<FnPackedForwardMode>(serialize, options);
            FnLogger.Info(deserialize.Tag);
            foreach (var e in deserialize.Entries)
            {
                FnLogger.Info(e.EventTime.ToString("O"));
                FnLogger.Info(e.Record["message"]);
            }

            FnLogger.Info(deserialize.Option);
            var deserialize2 =
                (FnPackedForwardMode) MessagePackSerializer.Deserialize<BaseFnEventMode>(serialize, options);
            FnLogger.Info(deserialize2.Tag);
            foreach (var e in deserialize2.Entries)
            {
                FnLogger.Info(e.EventTime.ToString("O"));
                FnLogger.Info(e.Record["message"]);
            }

            FnLogger.Info(deserialize2.Option);
        }

        [Fact]
        public void TestCompressedPackedForwardMode()
        {
            var compressedPackedForwardMode = new FnCompressedPackedForwardMode
            {
                Tag = "tag.name"
            };
            var entries = new List<FnEntry>();
            var entry = new FnEntry
            {
                EventTime = new DateTime(2020, 1, 2, 3, 4, 5, 123).AddTicks(4567),
                Record = new Dictionary<string, object> { { "message", "foo" } }
            };
            entries.Add(entry);
            compressedPackedForwardMode.Entries = entries;

            var resolver = CompositeResolver.Create(
                FnEventModeResolver.Instance,
                StandardResolver.Instance
            );
            var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
            var serialize = MessagePackSerializer.Serialize(compressedPackedForwardMode, options);
            var msg = BitConverter.ToString(serialize).Replace("-", " ");
            FnLogger.Info(msg);
            var deserialize = MessagePackSerializer.Deserialize<FnCompressedPackedForwardMode>(serialize, options);
            FnLogger.Info(deserialize.Tag);
            foreach (var e in deserialize.Entries)
            {
                FnLogger.Info(e.EventTime.ToString("O"));
                FnLogger.Info(e.Record["message"]);
            }

            FnLogger.Info(deserialize.Option);
            var deserialize2 =
                (FnCompressedPackedForwardMode) MessagePackSerializer.Deserialize<BaseFnEventMode>(serialize, options);
            FnLogger.Info(deserialize2.Tag);
            foreach (var e in deserialize2.Entries)
            {
                FnLogger.Info(e.EventTime.ToString("O"));
                FnLogger.Info(e.Record["message"]);
            }

            FnLogger.Info(deserialize2.Option);

            int? a = null;
            FnLogger.Error(a.HasValue);
        }
    }
}