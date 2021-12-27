using System;
using System.Collections.Generic;
using FluentNest.Formatters;
using MessagePack;

namespace FluentNest
{
    [MessagePackObject]
    public class FnMsgpackOutHelo
    {
        [Key(0)]
        public string Type { get; } = "HELO";

        [Key(1)]
        public FnMsgpackOutHeloOption Option { get; set; } = new();
    }

    [MessagePackObject]
    public class FnMsgpackOutHeloOption
    {
        [Key("nonce")]
        public string Nonce { get; set; } = null!;

        [Key("auth")]
        public string Auth { get; set; } = string.Empty;

        [Key("keepalive")]
        public bool Keepalive { get; set; } = true;
    }

    [MessagePackObject]
    public class FnMsgpackInPing
    {
        [Key(0)]
        public string Type { get; set; }

        [Key(1)]
        public string ClientHostname { get; set; }

        [Key(2)]
        public string ShareKeySalt { get; set; }

        [Key(3)]
        public string ShareKeyHexdigest { get; set; }

        [Key(4)]
        public string Username { get; set; }

        [Key(5)]
        public string Password { get; set; }
    }

    [MessagePackObject]
    public class FnMsgpackOutPong
    {
        [Key(0)]
        public string Type { get; } = "PONG";

        [Key(1)]
        public bool AuthResult { get; set; }

        [Key(2)]
        public string Reason { get; set; }

        [Key(3)]
        public string ServerHostname { get; set; }

        [Key(4)]
        public string SharedKeyHexdigest { get; set; }
    }

    [MessagePackObject]
    public class FnMsgpackInMessageMode
    {
        [Key(0)]
        public string Tag { get; set; }

        [Key(1)]
        public byte[] EventTime { get; set; }

        [Key(2)]
        public Dictionary<string, object> Record { get; set; }

        [Key(3)]
        public FnMsgpackInOption? Option { get; set; }
    }

    [MessagePackObject]
    public class FnMsgpackInForwardMode
    {
        [Key(0)]
        public string Tag { get; set; }

        [Key(1)]
        public byte[] Entries { get; set; }

        [Key(2)]
        public FnMsgpackInOption? Option { get; set; }
    }

    [MessagePackObject]
    public class FnMsgpackInEntry
    {
        [Key(0)]
        public long EventTime { get; set; }

        [Key(1)]
        public Dictionary<string, object> Record { get; set; }
    }

    [MessagePackObject]
    public class FnMsgpackInOption
    {
        [Key("size")]
        public int? Size { get; set; }

        [Key("chunk")]
        public string? Chunk { get; set; }

        [Key("compressed")]
        public string? Compressed { get; set; }
    }

    [MessagePackObject]
    public class FnMsgpackOutAck
    {
        [Key("ack")]
        public string Ack { get; set; }
    }

    public abstract class BaseFnEventMode
    {
        [Key(0)]
        public string Tag { get; set; }
    }

    [MessagePackObject]
    public class FnMessageMode : BaseFnEventMode
    {
        [Key(1)]
        [MessagePackFormatter(typeof(FnEventTimeFormatter))]
        public DateTime EventTime { get; set; }

        [Key(2)]
        public Dictionary<string, object> Record { get; set; }

        [Key(3)]
        public Dictionary<string, object>? Option { get; set; }

        [IgnoreMember]
        public int? Size
        {
            get
            {
                if (Option is not null && Option.TryGetValue("size", out var value))
                {
                    return Convert.ToInt32(value);
                }

                return null;
            }
        }

        [IgnoreMember]
        public string? Chunk
        {
            get
            {
                if (Option is not null && Option.TryGetValue("chunk", out var value))
                {
                    return (string) value;
                }

                return null;
            }
        }

        [IgnoreMember]
        public string? Compressed
        {
            get
            {
                if (Option is not null && Option.TryGetValue("compressed", out var value))
                {
                    return (string) value;
                }

                return null;
            }
        }
    }

    [MessagePackObject]
    public class FnEntry
    {
        [Key(0)]
        [MessagePackFormatter(typeof(FnEventTimeFormatter))]
        public DateTime EventTime { get; set; }

        [Key(1)]
        public Dictionary<string, object> Record { get; set; }
    }

    [MessagePackObject]
    public class FnForwardMode : BaseFnEventMode
    {
        [Key(1)]
        public List<FnEntry> Entries { get; set; }

        [Key(2)]
        public Dictionary<string, object>? Option { get; set; }

        [IgnoreMember]
        public int? Size
        {
            get
            {
                if (Option is not null && Option.TryGetValue("size", out var value))
                {
                    return Convert.ToInt32(value);
                }

                return null;
            }
        }

        [IgnoreMember]
        public string? Chunk
        {
            get
            {
                if (Option is not null && Option.TryGetValue("chunk", out var value))
                {
                    return (string) value;
                }

                return null;
            }
        }

        [IgnoreMember]
        public string? Compressed
        {
            get
            {
                if (Option is not null && Option.TryGetValue("compressed", out var value))
                {
                    return (string) value;
                }

                return null;
            }
        }
    }

    [MessagePackObject]
    public class FnPackedForwardMode : BaseFnEventMode
    {
        [Key(1)]
        [MessagePackFormatter(typeof(FnEventStreamFormatter))]
        public List<FnEntry> Entries { get; set; }

        [Key(2)]
        public Dictionary<string, object>? Option { get; set; }

        [IgnoreMember]
        public int? Size
        {
            get
            {
                if (Option is not null && Option.TryGetValue("size", out var value))
                {
                    return Convert.ToInt32(value);
                }

                return null;
            }
        }

        [IgnoreMember]
        public string? Chunk
        {
            get
            {
                if (Option is not null && Option.TryGetValue("chunk", out var value))
                {
                    return (string) value;
                }

                return null;
            }
        }

        [IgnoreMember]
        public string? Compressed
        {
            get
            {
                if (Option is not null && Option.TryGetValue("compressed", out var value))
                {
                    return (string) value;
                }

                return null;
            }
        }
    }

    [MessagePackObject]
    public class FnCompressedPackedForwardMode : BaseFnEventMode
    {
        [Key(1)]
        [MessagePackFormatter(typeof(FnCompressedEventStreamFormatter))]
        public List<FnEntry> Entries { get; set; }

        [Key(2)]
        public Dictionary<string, object> Option { get; set; } =
            new() { { "compressed", "gzip" } };

        [IgnoreMember]
        public int? Size
        {
            get
            {
                if (Option.TryGetValue("size", out var value))
                {
                    return Convert.ToInt32(value);
                }

                return null;
            }
        }

        [IgnoreMember]
        public string? Chunk
        {
            get
            {
                if (Option.TryGetValue("chunk", out var value))
                {
                    return (string) value;
                }

                return null;
            }
        }

        [IgnoreMember]
        public string? Compressed
        {
            get
            {
                if (Option.TryGetValue("compressed", out var value))
                {
                    return (string) value;
                }

                return null;
            }
        }
    }
}