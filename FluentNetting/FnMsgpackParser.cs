using System;
using System.Buffers;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Resolvers;

namespace FluentNetting
{
    /// <summary>
    ///     Parser.
    /// </summary>
    internal static class FnMsgpackParser
    {
        /// <summary>
        ///     Resolver.
        /// </summary>
        private static readonly IFormatterResolver Resolver = CompositeResolver.Create(
            FnEventModeResolver.Instance,
            StandardResolver.Instance
        );

        /// <summary>
        ///     Try parse for ping.
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="ping">out FnPing object</param>
        /// <returns>If parsed success, return true</returns>
        internal static bool TryParseForPing(byte[] message, out FnPing? ping)
        {
            // FnLogger.Debug(() => $"TryParseForPing: {message.FxToHexString()}");
            
            // init
            ping = null;

            try
            {
                // reader
                var reader = new MessagePackReader(message);
            
                // check PING format
                if (!reader.TryReadArrayHeader(out var count) || count != 6)
                {
                    return false;
                }
            
                // 1st value
                if (reader.ReadString() != "PING")
                {
                    return false;
                }
            
                // 2nd value(use for calculate digest)
                var clientHostname = reader.ReadStringSequence();
            
                // 3rd value(use for calculate digest)
                ReadOnlySequence<byte>? sharedKeySalt;
                switch (reader.NextCode)
                {
                    case MessagePackCode.Str8:
                        sharedKeySalt = reader.ReadStringSequence();
                        break;
                    case MessagePackCode.Bin8:
                    case MessagePackCode.Bin16:
                    case MessagePackCode.Bin32:
                    case MessagePackCode.Str16:
                    case MessagePackCode.Str32:
                        sharedKeySalt = reader.ReadBytes();
                        break;
                    default:
                        if (reader.NextCode >= MessagePackCode.MinFixStr && reader.NextCode <= MessagePackCode.MaxFixStr)
                        {
                            sharedKeySalt = reader.ReadBytes();
                            break;
                        }

                        return false;
                }

                // 4th value
                var sharedKeyHexdigest = reader.ReadStringSequence();

                // 5th value
                ReadOnlySequence<byte>? username = reader.ReadStringSequence();
            
                // 6th value
                ReadOnlySequence<byte>? password = reader.ReadStringSequence();
                
                ping = new FnPing()
                {
                    Type = "PING",
                    ClientHostname = clientHostname,
                    SharedKeySalt = sharedKeySalt,
                    SharedKeyHexdigest = sharedKeyHexdigest,
                    Username = username,
                    Password = password
                };
#if DEBUG
                var cpPing = ping;
                FnLogger.Debug(() => $"Parsed success: {cpPing}");
#endif
                return true;
            }
            catch (Exception e)
            {
                FnLogger.Debug(() => $"Parsed error: {message.FxToString()}");
                FnLogger.Debug(() => e);
            }

            // deinit
            ping = null;
            return false;
        }
        
        /// <summary>
        ///     Try parse for 4 modes messages.
        ///     * message mode
        ///     * forward mode
        ///     * packed forward mode
        ///     * compressed packed forward mode
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="msg">out FnMessage object</param>
        /// <returns>If parsed success, return true</returns>
        internal static bool TryParseForMessage(byte[] message, out FnMessage? msg)
        {
            FnLogger.Debug(() => $"TryParseForMessage: {message.FxToHexString()}");
            
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
                FnLogger.Debug(() => e);
            }

            // deinit
            msg = null;
            return false;
        }
    }
}