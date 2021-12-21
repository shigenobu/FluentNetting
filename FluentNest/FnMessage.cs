using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

namespace FluentNest
{
    public class FnMessage
    {
        public string Tag { get; internal set; }

        public List<FnMessageEntry>? Entries { get; internal set; }

        public override string ToString()
        {
            var entries = string.Empty;
            if (Entries != null) entries = string.Join(",", Entries.Select(e => e.ToString()));
            return $"Tag:{Tag}, Entries:[{entries}]";
        }
    }

    public class FnMessageEntry
    {
        public DateTime EventTime { get; internal set; }
        
        public Dictionary<string, object>? Record { get; internal set; }
        
        public override string ToString()
        {
            var records = string.Empty;
            if (Record != null) records = string.Join(",", Record.Select(e => $"{e.Key}:{e.Value}"));
            return $"EventTime:{EventTime}, Record:[{records}]";
        }
    }
}