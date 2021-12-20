using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using MessagePack;
using PurpleSofa;

namespace FluentNest
{
    internal class FnMsgpackParser
    {
        private const string TmpStoredKey = "__fluentNestPartialMessage";
        
        internal FnMessage? Unpack(PsSession session, byte[] message)
        {
            byte[] newMessage = Array.Empty<byte>();
            var prevMessage = session.GetValue<byte[]>(TmpStoredKey);
            if (prevMessage != null)
            {
                newMessage = prevMessage.FxConcat(prevMessage);
            }
            else
            {
                newMessage = message;
            }

            try
            {
                var serialized = MessagePackSerializer.Deserialize<FnMsgpackForward>(newMessage);
                var serializedEntries = Convert(serialized.Entries);
                var entries = new List<FnMessageEntry>();
                foreach (var serializedEntry in serializedEntries)
                {
                    entries.Add(new FnMessageEntry()
                    {
                        EventTime = DateTimeOffset.FromUnixTimeSeconds(serializedEntry.EventTime).DateTime,
                        Records = serializedEntry.Record
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
                // TODO error type
                session.SetValue(TmpStoredKey, newMessage);
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
                        long unixSeconds = seconds + 1000 * nanoSeconds;

                        // replace byte array
                        var newMsgpack = msgpack.Slice(0, 1).ToArray()
                            .FxConcat(BitConverter.GetBytes(unixSeconds))
                            .FxConcat(msgpack.Slice(11).ToArray());
                        
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