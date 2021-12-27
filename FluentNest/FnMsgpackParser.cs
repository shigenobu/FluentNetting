using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using MessagePack;
using MessagePack.Resolvers;

namespace FluentNest
{
    internal class FnMsgpackParser
    {
        private static readonly IFormatterResolver Resolver = CompositeResolver.Create(
            FnEventModeResolver.Instance,
            StandardResolver.Instance
        );
        
        internal static bool TryParse(byte[] message, out FnMessage? msg)
        {
            FnLogger.Debug(() => $"TryParse: {message.FxToHexString()}");
            
            try
            {
                // deserialize
                var m = MessagePackSerializer.Deserialize<BaseFnEventMode>(message,
                    MessagePackSerializerOptions.Standard.WithResolver(Resolver));

                // init
                msg = new FnMessage
                {
                    Tag = m.Tag
                };

                // check type
                var entries = new List<FnMessageEntry>();
                switch (m)
                {
                    case FnMessageMode o:
                        entries.Add(new FnMessageEntry
                        {
                            EventTime = o.EventTime,
                            Record = o.Record
                        });
                        msg.Option = new FnMessageOption
                        {
                            Size = o.Size,
                            Chunk = o.Chunk,
                            Compressed = o.Compressed
                        };
                        break;
                    case FnForwardMode o:
                        foreach (var e in o.Entries)
                        {
                            entries.Add(new FnMessageEntry
                            {
                                EventTime = e.EventTime,
                                Record = e.Record
                            });
                        }
                        msg.Option = new FnMessageOption
                        {
                            Size = o.Size,
                            Chunk = o.Chunk,
                            Compressed = o.Compressed
                        };
                        break;
                    case FnPackedForwardMode o:
                        foreach (var e in o.Entries)
                        {
                            entries.Add(new FnMessageEntry
                            {
                                EventTime = e.EventTime,
                                Record = e.Record
                            });
                        }
                        msg.Option = new FnMessageOption
                        {
                            Size = o.Size,
                            Chunk = o.Chunk,
                            Compressed = o.Compressed
                        };
                        break;
                    case FnCompressedPackedForwardMode o:
                        foreach (var e in o.Entries)
                        {
                            entries.Add(new FnMessageEntry
                            {
                                EventTime = e.EventTime,
                                Record = e.Record
                            });
                        }
                        msg.Option = new FnMessageOption
                        {
                            Size = o.Size,
                            Chunk = o.Chunk,
                            Compressed = o.Compressed
                        };
                        break;
                }
                msg.Entries = entries;
#if DEBUG
                var cpMsg = msg;
                FnLogger.Debug(() => $"Parsed success by '{m.GetType()}': {cpMsg}");
#endif
                return true;
            }
            catch (Exception e)
            {
                FnLogger.Debug(() => $"Parsed error: {message.FxToString()}");
            }

            // deinit
            msg = null;
            return false;
        }
    }
}