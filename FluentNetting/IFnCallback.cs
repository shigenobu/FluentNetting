using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentNetting;

/// <summary>
///     Callback interface.
/// </summary>
public interface IFnCallback
{
    /// <summary>
    ///     Async receive.
    ///     Call 'Receive' at default.
    /// </summary>
    /// <param name="tag">tag</param>
    /// <param name="entries">entries</param>
    /// <returns>task</returns>
    public Task ReceiveAsync(string tag, List<FnMessageEntry> entries);
}