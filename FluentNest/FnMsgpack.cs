using System.Collections.Generic;
using MessagePack;

namespace FluentNest
{
    [MessagePackObject]
    public class FnMsgpackInHelo
    {
        [Key(0)]
        public string Type { get; } = "HELO";

        [Key(1)]
        public FnMsgpackInHeloOption Option { get; set; } = new();
    }
    
    [MessagePackObject]
    public class FnMsgpackInHeloOption
    {
        [Key("nonce")]
        public string Nonce { get; set; } = null!;

        [Key("auth")]
        public string Auth { get; set; } = string.Empty;

        [Key("keepalive")]
        public bool Keepalive { get; set; } = true;
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
}