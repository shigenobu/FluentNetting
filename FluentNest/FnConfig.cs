using System;

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
        internal bool CheckDigest(FnMsgpackInPing ping)
        {
            var src = $"{ping.ShareKeySalt}{ping.ClientHostname}{Nonce}{SharedKey}";
            var digest = src.FxSha512();
            return digest.Equals(ping.ShareKeyHexdigest);
        }

        /// <summary>
        ///     Create digest.
        ///     If Authorization enable, used by create digest.
        /// </summary>
        /// <param name="ping">ping</param>
        /// <returns>server created digest</returns>
        internal string CreateDigest(FnMsgpackInPing ping)
        {
            var hash = $"{ping.ShareKeySalt}{ServerHostname}{Nonce}{SharedKey}";
            return hash.FxSha512();
        }
    }
}