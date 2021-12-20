using System.Collections.Generic;
using MessagePack;

namespace FluentNest
{
    [MessagePackObject]
    internal class FnMsgpackHelo
    {
        [Key(0)]
        internal string Type { get; } = "HELO";

        [Key(1)]
        internal FnMsgpackHeloOption Option { get; set; } = new();
    }
    
    [MessagePackObject]
    internal class FnMsgpackHeloOption
    {
        [Key("nonce")]
        internal string Nonce { get; set; } = null!;

        [Key("auth")]
        internal string Auth { get; set; } = string.Empty;

        [Key("keepalive")]
        internal bool Keepalive { get; set; } = true;
    }

    [MessagePackObject]
    internal class FnMsgpackForward
    {
        [Key(0)]
        internal string Tag { get; set; }
        
        [Key(1)]
        internal byte[] Entries { get; set; }
        
        [Key(2)]
        internal FnMsgpackForwardOption? Option { get; set; }
    }
    
    [MessagePackObject]
    internal class FnMsgpackForwardEntry
    {
        [Key(0)]
        internal long EventTime { get; set; }

        [Key(2)]
        internal Dictionary<string, object> Record { get; set; }
    }

    [MessagePackObject]
    internal class FnMsgpackForwardOption
    {
        [Key("size")]
        internal int? Size { get; set; }

        [Key("chunk")]
        internal string? Chunk { get; set; }
        
        [Key("compressed")]
        internal string? Compressed { get; set; }
    }
}