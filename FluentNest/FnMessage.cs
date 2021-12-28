using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentNest
{
    /// <summary>
    ///     Fluent message object.
    /// </summary>
    public class FnMessage
    {
        /// <summary>
        ///     Tag.
        /// </summary>
        public string Tag { get; internal set; } = null!;

        /// <summary>
        ///     Entries.
        ///     User data.
        /// </summary>
        public List<FnMessageEntry>? Entries { get; internal set; }
        
        /// <summary>
        ///     Option.
        /// </summary>
        public FnMessageOption? Option { get; internal set; }

        /// <summary>
        ///     To string.
        /// </summary>
        /// <returns>string expression</returns>
        public override string ToString()
        {
            var entries = string.Empty;
            if (Entries != null) entries = string.Join(",", Entries.Select(e => e.ToString()));
            return $"Tag:{Tag}, Entries:[{entries}], Option:{Option}";
        }
    }

    /// <summary>
    ///     Fluent message entry.
    /// </summary>
    public class FnMessageEntry
    {
        /// <summary>
        ///     Event time.
        /// </summary>
        public DateTimeOffset EventTime { get; internal set; }
        
        /// <summary>
        ///     Record.
        /// </summary>
        public Dictionary<string, object>? Record { get; internal set; }
        
        /// <summary>
        ///     To string.
        /// </summary>
        /// <returns>string expression</returns>
        public override string ToString()
        {
            var records = string.Empty;
            if (Record != null) records = string.Join(",", Record.Select(e => $"{e.Key}:{e.Value}"));
            return $"EventTime:{EventTime}, Record:[{records}]";
        }
    }
    
    /// <summary>
    ///     Fluent message option.
    /// </summary>
    public class FnMessageOption
    {
        /// <summary>
        ///     Size.
        ///     Size of array for stream.
        /// </summary>
        public int? Size { get; internal set; }

        /// <summary>
        ///     Chunk.
        ///     Used in ack returning.
        /// </summary>
        public string? Chunk { get; internal set; }
        
        /// <summary>
        ///     Compressed.
        ///     Compression format info.
        /// </summary>
        public string? Compressed { get; internal set; }

        /// <summary>
        ///     To string.
        /// </summary>
        /// <returns>string expression</returns>
        public override string ToString()
        {
            return $"Size:{Size}, Chunk:{Chunk}, Compressed:{Compressed}";
        }
    }
}