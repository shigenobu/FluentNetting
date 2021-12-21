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
        internal FnMessage? Unpack(byte[] message)
        {
            try
            {
                var serialized = MessagePackSerializer.Deserialize<FnMsgpackForward>(message);
                var serializedEntries = Convert(serialized.Entries);
                var entries = new List<FnMessageEntry>();
                foreach (var serializedEntry in serializedEntries)
                {
                    entries.Add(new FnMessageEntry()
                    {
                        EventTime = DateTimeOffset.FromUnixTimeMilliseconds(serializedEntry.EventTime).DateTime,
                        Record = serializedEntry.Record
                    });
                }

                return new FnMessage()
                {
                    Tag = serialized.Tag,
                    Entries = entries
                };
            }
            catch (Exception e)
            {
                throw;
            }
            
            return null;
        }
        
        private List<FnMsgpackForwardEntry> Convert(byte[] entries)
        {
            var list = new List<FnMsgpackForwardEntry>();
            using (var reader = new MessagePackStreamReader(new MemoryStream(entries)))
            {
                while (reader.ReadAsync(CancellationToken.None).Result is { } msgpack)
                {
                    // check D7 00
                    var head2 = msgpack.Slice(1, 2).ToArray();
                    if (head2[0] == 0xD7 && head2[1] == 0x00)
                    {
                        // 10 byte event time
                        int seconds = BitConverter.ToInt32(msgpack.Slice(3, 4).ToArray().Reverse().ToArray());
                        int nanoSeconds = BitConverter.ToInt32(msgpack.Slice(7, 4).ToArray().Reverse().ToArray());
                        long unixMilliSeconds = (long)seconds * 1000 + (long)nanoSeconds / 1000;

                        // replace byte array
                        var newMsgpack = msgpack.Slice(0, 1).ToArray()
                            .FxConcat(new byte[]{0xd3})
                            .FxConcat(BitConverter.GetBytes(unixMilliSeconds).Reverse().ToArray())
                            .FxConcat(msgpack.Slice(11).ToArray());
                        
                        // StringBuilder builder = new StringBuilder();
                        // foreach (var b in newMsgpack.ToArray())
                        // {
                        //     builder.Append(b.ToString("X") + " ");
                        // }
                        // FnLogger.Debug(builder.ToString());
                        
                        // add
                        list.Add(MessagePackSerializer.Deserialize<FnMsgpackForwardEntry>(newMsgpack));
                        continue;
                    }
                    
                    // check C7  08  00
                    var head3 = msgpack.Slice(1, 3).ToArray();
                    if (head3[0] == 0xC7 && head3[1] == 0x08 && head3[2] == 0x00 )
                    {
                        // 11 byte event time
                    }
                }
            }

            return list;
        }
    }
}