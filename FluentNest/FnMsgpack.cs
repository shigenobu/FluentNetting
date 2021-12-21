using System.Collections.Generic;
using MessagePack;

namespace FluentNest
{
    [MessagePackObject]
    public class FnMsgpackHelo
    {
        [Key(0)]
        public string Type { get; } = "HELO";

        [Key(1)]
        public FnMsgpackHeloOption Option { get; set; } = new();
    }
    
    [MessagePackObject]
    public class FnMsgpackHeloOption
    {
        [Key("nonce")]
        public string Nonce { get; set; } = null!;

        [Key("auth")]
        public string Auth { get; set; } = string.Empty;

        [Key("keepalive")]
        public bool Keepalive { get; set; } = true;
    }

    [MessagePackObject]
    public class FnMsgpackForward
    {
        [Key(0)]
        public string Tag { get; set; }
        
        [Key(1)]
        public byte[] Entries { get; set; }
        
        [Key(2)]
        public FnMsgpackForwardOption? Option { get; set; }
    }
    
    [MessagePackObject]
    public class FnMsgpackForwardEntry
    {
        [Key(0)]
        public long EventTime { get; set; }

        [Key(1)]
        public Dictionary<string, object> Record { get; set; }
    }

    [MessagePackObject]
    public class FnMsgpackForwardOption
    {
        [Key("size")]
        public int? Size { get; set; }

        [Key("chunk")]
        public string? Chunk { get; set; }
        
        [Key("compressed")]
        public string? Compressed { get; set; }
    }
}