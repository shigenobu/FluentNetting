using System;
using System.Buffers;
using System.IO;

namespace FluentNest
{
    /// <summary>
    ///     Config.
    /// </summary>
    public class FnConfig
    {
        /// <summary>
        ///     Share key.
        ///     If 'security' section used.
        ///     Client and server must be the same value.
        /// </summary>
        public string? SharedKey { get; set; }

        /// <summary>
        ///     Nonce.
        ///     If 'security' section used.
        ///     Server allow to set free.
        /// </summary>
        public string? Nonce { get; set; }

        /// <summary>
        ///     Server host name.
        ///     If 'security' section used.
        ///     Server allow to set free.
        /// </summary>
        public string ServerHostname { get; set; } = Environment.MachineName;

        /// <summary>
        ///     Require ack, default true.
        /// </summary>
        public bool RequireAck { get; set; } = true;

        /// <summary>
        ///     If keepalive used, set true, default false.
        ///     Used by authorization.
        /// </summary>
        public bool KeepAlive { get; set; }

        /// <summary>
        ///     Enable authorization.
        ///     If 'ShareKey' and 'Nonce' is not null, return true.
        /// </summary>
        /// <returns>'ShareKey' and 'Nonce' is not null, return true.</returns>
        internal bool EnableAuthorization()
        {
            return !string.IsNullOrEmpty(SharedKey) && !string.IsNullOrEmpty(Nonce);
        }

        /// <summary>
        ///     Check digest.
        ///     If Authorization enable, used by check digest.
        /// </summary>
        /// <param name="ping">ping</param>
        /// <returns>client digest is equal to server digest , return true</returns>
        internal bool CheckDigest(FnPing ping)
        {
            // check value
            if (!ping.SharedKeySalt.HasValue) return false;
            if (!ping.ClientHostname.HasValue) return false;
            if (!ping.SharedKeyHexdigest.HasValue) return false;
            
            // verify digest
            using var memoryStream = new MemoryStream();
            memoryStream.Write(ping.SharedKeySalt.Value.ToArray());
            memoryStream.Write(ping.ClientHostname.Value.ToArray());
            memoryStream.Write(Nonce!.FxToBytes());
            memoryStream.Write(SharedKey!.FxToBytes());
            memoryStream.Seek(0, SeekOrigin.Begin);
            var data = memoryStream.ToArray();
            var digest = data.FxSha512();

            FnLogger.Debug(() => $"Digest client/server: " +
                                 $"{ping.SharedKeyHexdigest.Value.ToArray().FxToString()}/{digest}");
            
            return ping.SharedKeyHexdigest.Value.ToArray().FxToString() == digest;
        }

        /// <summary>
        ///     Create digest.
        ///     If Authorization enable, used by create digest.
        /// </summary>
        /// <param name="ping">ping</param>
        /// <returns>server created digest</returns>
        internal string CreateDigest(FnPing ping)
        {
            var hash = $"{ping.SharedKeySalt!.Value.ToArray().FxToString()}{ServerHostname}{Nonce}{SharedKey}";
            return hash.FxSha512();
        }
    }
}