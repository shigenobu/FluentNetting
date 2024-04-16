using System;
using System.Collections.Generic;
using FluentNetting.Formatters;
using MessagePack;

namespace FluentNetting;

[MessagePackObject]
public class FnMsgpackOutHelo
{
    [SerializationConstructor]
    public FnMsgpackOutHelo()
    {
    }

    public FnMsgpackOutHelo(string nonce, string? auth, bool keepalive)
    {
        Nonce = nonce;
        if (!string.IsNullOrEmpty(auth))
            // In Fluent Bit, if set some value (include null or empty string), then password that is in PING message is not empty string.
            Auth = auth;

        Keepalive = keepalive;
    }

    [Key(0)] public string Type { get; } = "HELO";

    [Key(1)] public Dictionary<string, object> Option { get; set; } = new();

    [IgnoreMember]
    public string Nonce
    {
        get
        {
            if (Option.TryGetValue("nonce", out var val)) return (string) val;

            return string.Empty;
        }
        set => Option["nonce"] = value;
    }

    [IgnoreMember]
    public string Auth
    {
        get
        {
            if (Option.TryGetValue("auth", out var val)) return (string) val;

            return string.Empty;
        }
        set => Option["auth"] = value;
    }

    [IgnoreMember]
    public bool Keepalive
    {
        get
        {
            if (Option.TryGetValue("keepalive", out var val)) return (bool) val;

            return true;
        }
        set => Option["keepalive"] = value;
    }
}

[MessagePackObject]
public class FnMsgpackOutPong
{
    [Key(0)] public string Type { get; } = "PONG";

    [Key(1)] public bool AuthResult { get; set; }

    [Key(2)] public string Reason { get; set; }

    [Key(3)] public string ServerHostname { get; set; }

    [Key(4)] public string SharedKeyHexdigest { get; set; }
}

[MessagePackObject]
public class FnMsgpackOutAck
{
    [Key("ack")] public string Ack { get; set; }
}

public abstract class BaseFnEventMode
{
    [Key(0)] public string Tag { get; set; }
}

[MessagePackObject]
public class FnMessageMode : BaseFnEventMode
{
    [Key(1)]
    [MessagePackFormatter(typeof(FnEventTimeFormatter))]
    public DateTimeOffset EventTime { get; set; }

    [Key(2)] public Dictionary<string, object> Record { get; set; }

    [Key(3)] public Dictionary<string, object>? Option { get; set; }

    [IgnoreMember]
    public int? Size
    {
        get
        {
            if (Option is not null && Option.TryGetValue("size", out var value)) return Convert.ToInt32(value);

            return null;
        }
    }

    [IgnoreMember]
    public string? Chunk
    {
        get
        {
            if (Option is not null && Option.TryGetValue("chunk", out var value)) return (string) value;

            return null;
        }
    }

    [IgnoreMember]
    public string? Compressed
    {
        get
        {
            if (Option is not null && Option.TryGetValue("compressed", out var value)) return (string) value;

            return null;
        }
    }
}

[MessagePackObject]
public class FnEntry
{
    [Key(0)]
    [MessagePackFormatter(typeof(FnEventTimeFormatter))]
    public DateTimeOffset EventTime { get; set; }

    [Key(1)] public Dictionary<string, object> Record { get; set; }
}

[MessagePackObject]
public class FnForwardMode : BaseFnEventMode
{
    [Key(1)] public List<FnEntry> Entries { get; set; }

    [Key(2)] public Dictionary<string, object>? Option { get; set; }

    [IgnoreMember]
    public int? Size
    {
        get
        {
            if (Option is not null && Option.TryGetValue("size", out var value)) return Convert.ToInt32(value);

            return null;
        }
    }

    [IgnoreMember]
    public string? Chunk
    {
        get
        {
            if (Option is not null && Option.TryGetValue("chunk", out var value)) return (string) value;

            return null;
        }
    }

    [IgnoreMember]
    public string? Compressed
    {
        get
        {
            if (Option is not null && Option.TryGetValue("compressed", out var value)) return (string) value;

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

    [Key(2)] public Dictionary<string, object>? Option { get; set; }

    [IgnoreMember]
    public int? Size
    {
        get
        {
            if (Option is not null && Option.TryGetValue("size", out var value)) return Convert.ToInt32(value);

            return null;
        }
    }

    [IgnoreMember]
    public string? Chunk
    {
        get
        {
            if (Option is not null && Option.TryGetValue("chunk", out var value)) return (string) value;

            return null;
        }
    }

    [IgnoreMember]
    public string? Compressed
    {
        get
        {
            if (Option is not null && Option.TryGetValue("compressed", out var value)) return (string) value;

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
        new() {{"compressed", "gzip"}};

    [IgnoreMember]
    public int? Size
    {
        get
        {
            if (Option.TryGetValue("size", out var value)) return Convert.ToInt32(value);

            return null;
        }
    }

    [IgnoreMember]
    public string? Chunk
    {
        get
        {
            if (Option.TryGetValue("chunk", out var value)) return (string) value;

            return null;
        }
    }

    [IgnoreMember]
    public string? Compressed
    {
        get
        {
            if (Option.TryGetValue("compressed", out var value)) return (string) value;

            return null;
        }
    }
}