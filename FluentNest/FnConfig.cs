using System;

namespace FluentNest
{
    public class FnConfig
    {
        public string? SharedKey { get; set; }
        
        public string? Nonce { get; set; }

        public string ServerHostname { get; set; } = Environment.MachineName;
        
        public bool RequireAck { get; set; } = true;
        
        public bool KeepAlive { get; set; }

        public bool EnableAuthorization()
        {
            return !string.IsNullOrEmpty(SharedKey) && !string.IsNullOrEmpty(Nonce);
        }

        public bool CheckDigest(FnMsgpackInPing ping)
        {
            var src = $"{ping.ShareKeySalt}{ping.ClientHostname}{Nonce}{SharedKey}";
            var digest = src.FxSha512();
            return digest.Equals(ping.ShareKeyHexdigest);
        }
    }
}