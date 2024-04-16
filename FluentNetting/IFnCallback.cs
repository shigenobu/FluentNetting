using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FluentNetting;

/// <summary>
///     Callback interface.
/// </summary>
public interface IFnCallback
{
    /// <summary>
    ///     Synchronous method names.
    /// </summary>
    internal static readonly List<string> SynchronousMethodNames = new() {"Receive"};

    /// <summary>
    ///     Contains async.
    /// </summary>
    /// <param name="callback">callback</param>
    /// <returns>if contains, return true</returns>
    internal static bool ContainsAsync(IFnCallback callback)
    {
        var attType = typeof(AsyncStateMachineAttribute);
        foreach (var methodInfo in callback.GetType().GetMethods())
        {
            if (!SynchronousMethodNames.Contains(methodInfo.Name)) continue;

            var attrib = methodInfo.GetCustomAttribute(attType);
            if (attrib != null) return true;
        }

        return false;
    }

    /// <summary>
    ///     Receive.
    ///     If use 'async', overrider 'ReceiveAsync' method alternatively.
    /// </summary>
    /// <param name="tag">tag</param>
    /// <param name="entries">entries</param>
    public void Receive(string tag, List<FnMessageEntry> entries);

    /// <summary>
    ///     Async receive.
    ///     Call 'Receive' at default.
    /// </summary>
    /// <param name="tag">tag</param>
    /// <param name="entries">entries</param>
    /// <returns>task</returns>
    public Task ReceiveAsync(string tag, List<FnMessageEntry> entries)
    {
        Receive(tag, entries);
        return Task.CompletedTask;
    }
}