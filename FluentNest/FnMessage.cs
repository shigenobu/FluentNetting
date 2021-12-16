using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;

namespace FluentNest
{
    [MessagePackObject]
    public class FnMessageHelo
    {
        [Key(0)]
        public string Type { get; } = "HELO";

        [Key(1)]
        public FnMessageHeloOption Option { get; set; } = new();
    }
    
    [MessagePackObject]
    public class FnMessageHeloOption
    {
        [Key("nonce")]
        public string Nonce { get; set; } = null!;

        [Key("auth")]
        public string Auth { get; set; } = string.Empty;

        [Key("keepalive")]
        public bool Keepalive { get; set; } = true;
    }

    [MessagePackObject]
    public class FnMessageForward
    {
        [Key(0)]
        public string Tag { get; set; }
        
        [Key(1)]
        // public FnMessageForwardEntry[] Entries { get; set; }
         // public FnMessageForwardEntry Entry { get; set; }
        // public int Time { get; set; }
        public byte[] Entries { get; set; }
        
        //
        // [Key(2)]
        // // public FnMessageForwardOption Option { get; set; }
        // public Dictionary<string, object> Record { get; set; }
        //
        [Key(2)]
        public FnMessageForwardOption? Option { get; set; }
    }
    
    [MessagePackObject]
    public class FnMessageForwardEntry
    {
        [Key(0)]
        [MessagePackFormatter(typeof(TypelessFormatter))]
        public object Time { get; set; }

        [Key(1)]
        public FnMessageForwardEntryError Error { get; set; }
        // public Dictionary<string, dynamic> Record { get; set; }
        // public byte[] Record { get; set; }
    }

    [MessagePackObject]
    public class FnMessageForwardEntryTime
    {
        
        
        [Key(0)]
        public int UnixNanoseconds { get; set; }
        
        [Key(1)]
        [MessagePackFormatter(typeof(TypelessFormatter))]
        public object Ext { get; set; }
    }

    [MessagePackObject]
    public class FnMessageForwardEntryError
    {
        [Key("retry_times")]
        public int RetryTimes { get; set; }
        
        [Key("records")]
        public int Records { get; set; }
        
        [Key("error")]
        public string Error { get; set; }
        
        [Key("message")]
        public string Message { get; set; }
    }
    
    [MessagePackObject]
    public class FnMessageForwardOption
    {
        [Key("size")]
        public int? Size { get; set; }

        [Key("chunk")]
        public string? Chunk { get; set; }
        
        [Key("compressed")]
        public string? Compressed { get; set; }

        public override string ToString()
        {
            return $"Size:{Size}, Chunk:{Chunk}, Compressed:{Compressed}";
        }
    }
}