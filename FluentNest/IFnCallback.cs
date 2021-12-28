using System.Collections.Generic;

namespace FluentNest
{
    /// <summary>
    ///     Callback interface.
    /// </summary>
    public interface IFnCallback
    {
        /// <summary>
        ///     Receive.
        /// </summary>
        /// <param name="tag">tag</param>
        /// <param name="entries">entries</param>
        public void Receive(string tag, List<FnMessageEntry> entries);
    }
}