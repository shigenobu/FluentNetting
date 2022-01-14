using System.Buffers;

namespace FluentNetting
{
    /// <summary>
    ///     Ping.
    /// </summary>
    internal class FnPing
    {
        /// <summary>
        ///     Type.
        /// </summary>
        internal string Type { get; init; } = null!;

        /// <summary>
        ///     Client hostname.
        /// </summary>
        internal ReadOnlySequence<byte>? ClientHostname { get; init; }

        /// <summary>
        ///     Shared key salt.
        /// </summary>
        internal ReadOnlySequence<byte>? SharedKeySalt { get; init; }

        /// <summary>
        ///     Shared key hex digest.
        /// </summary>
        internal ReadOnlySequence<byte>? SharedKeyHexdigest { get; init; }

        /// <summary>
        ///     User name.
        /// </summary>
        internal ReadOnlySequence<byte>? Username { get; init; }

        /// <summary>
        ///     Password.
        /// </summary>
        internal ReadOnlySequence<byte>? Password { get; init; }

        /// <summary>
        ///     To string.
        /// </summary>
        /// <returns>string expression.</returns>
        public override string ToString()
        {
            return $"Type:{Type}, " +
                   $"ClientHostname:{ClientHostname?.ToArray().FxToString()}, " +
                   $"SharedKeySalt:{SharedKeySalt?.ToArray().FxToString()}, " +
                   $"SharedKeyHexdigest:{SharedKeyHexdigest?.ToArray().FxToString()}, " +
                   $"Username:{Username?.ToArray().FxToString()}, " +
                   $"Password:{Password?.ToArray().FxToString()}";
        }
    }
}