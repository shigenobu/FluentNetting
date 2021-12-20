using System;
using System.Collections.Generic;

namespace FluentNest
{
    public class FnMessage
    {
        public string Tag { get; internal set; }

        public List<FnMessageEntry> Entries { get; internal set; }
    }

    public class FnMessageEntry
    {
        public DateTime EventTime { get; internal set; }
        
        public Dictionary<string, object> Records { get; internal set; }
    }
}