using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using MessagePack;

namespace FluentNest
{
    internal class FnMsgpackParser
    {
        internal static bool TryParse(byte[] message, out FnMessage? msg)
        {
            FnLogger.Debug(() => $"TryParse: {message.FxToHexString()}");
            
            // init null
            msg = null;
            
            // Forward Mode
            try
            {
                var serialized = MessagePackSerializer.Deserialize<FnMsgpackInForwardMode>(message);
                var serializedEntries = ConvertEntry(serialized.Entries);
                var entries = new List<FnMessageEntry>();
                foreach (var serializedEntry in serializedEntries)
                {
                    entries.Add(new FnMessageEntry()
                    {
                        EventTime = DateTimeOffset.FromUnixTimeMilliseconds(serializedEntry.EventTime),
                        Record = serializedEntry.Record
                    });
                }

                msg = new FnMessage()
                {
                    Tag = serialized.Tag,
                    Entries = entries,
                    Option = new FnMessageOption
                    {
                        Size = serialized.Option?.Size,
                        Chunk = serialized.Option?.Chunk,
                        Compressed = serialized.Option?.Compressed
                    }
                };
#if DEBUG
                var cpMsg = msg;
                FnLogger.Debug(() => $"Parsed success by 'Forward Mode': {cpMsg}");
#endif
                
                return true;
            }
            catch (Exception e)
            {
                FnLogger.Debug(() => $"Parsed error by 'Forward Mode': {message.FxToString()}");
            }
            
            // TODO really exists ?
            // Message Modes
            try
            {
                var serialized = MessagePackSerializer.Deserialize<FnMsgpackInMessageMode>(message);
                var unixMilliSeconds = ExtractEventTime(new ReadOnlySequence<byte>(serialized.EventTime), out _);
                var entries = new List<FnMessageEntry>();
                entries.Add(new FnMessageEntry
                {
                    EventTime = DateTimeOffset.FromUnixTimeMilliseconds(unixMilliSeconds),
                    Record = serialized.Record
                });

                msg = new FnMessage()
                {
                    Tag = serialized.Tag,
                    Entries = entries,
                    Option = new FnMessageOption
                    {
                        Size = serialized.Option?.Size,
                        Chunk = serialized.Option?.Chunk,
                        Compressed = serialized.Option?.Compressed
                    }
                };
#if DEBUG
                var cpMsg = msg;
                FnLogger.Debug(() => $"Parsed by 'Message Mode': {cpMsg}");
#endif
                
                return true;
            }
            catch (Exception e)
            {
                FnLogger.Debug(() => $"Parsed error by 'Message Mode': {message.FxToString()}");
            }
            
            // PackedForward Mode
            
            // CompressedPackedForward Mode
            
            return false;
        }
        
        private static List<FnMsgpackInEntry> ConvertEntry(byte[] entries)
        {
            var list = new List<FnMsgpackInEntry>();
            using (var reader = new MessagePackStreamReader(new MemoryStream(entries)))
            {
                while (reader.ReadAsync(CancellationToken.None).Result is { } msgpack)
                {
                    // 10 or 11 byte event time to 8 byte long
                    long unixMilliSeconds = ExtractEventTime(msgpack, out var len);
                    if (len == 0) continue;

                    // replace byte array
                    var newMsgpack = msgpack.Slice(0, 1).ToArray()
                        .FxConcat(new byte[]{0xD3})     // long
                        .FxConcat(BitConverter.GetBytes(unixMilliSeconds).Reverse().ToArray())
                        .FxConcat(msgpack.Slice(len + 1).ToArray());
                    FnLogger.Debug(() => $"ConvertEntry: {newMsgpack.FxToHexString()}");

                    // add
                    list.Add(MessagePackSerializer.Deserialize<FnMsgpackInEntry>(newMsgpack));
                }
            }

            return list;
        }


        private static long ExtractEventTime(ReadOnlySequence<byte> msgpack, out int len)
        {
            // head
            var head = msgpack.Slice(1, 3).ToArray();
            
            // check D7 00
            if (head[0] == 0xD7 && head[1] == 0x00)
            {
                // 10 byte event time to 8 byte long
                int seconds = BitConverter.ToInt32(msgpack.Slice(3, 4).ToArray().Reverse().ToArray());
                int nanoSeconds = BitConverter.ToInt32(msgpack.Slice(7, 4).ToArray().Reverse().ToArray());
                long unixMilliSeconds = (long)seconds * 1000 + (long)nanoSeconds / 1000;

                len = 10;
                return unixMilliSeconds;
            }

            // check C7 08 00
            if (head[0] == 0xC7 && head[1] == 0x08 && head[1] == 0x00)
            {
                // 11 byte event time to 8 byte long
                int seconds = BitConverter.ToInt32(msgpack.Slice(4, 4).ToArray().Reverse().ToArray());
                int nanoSeconds = BitConverter.ToInt32(msgpack.Slice(8, 4).ToArray().Reverse().ToArray());
                long unixMilliSeconds = (long)seconds * 1000 + (long)nanoSeconds / 1000;

                len = 11;
                return unixMilliSeconds;
            }
            
            // unknown
            len = 0;
            return 0;
        }
    }
}