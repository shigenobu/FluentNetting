using System;
using System.Buffers;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using MessagePack;

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
        ///     Check digest.
        ///     If Authorization enable, used by check digest.
        /// </summary>
        /// <param name="ping">ping</param>
        /// <returns>client digest is equal to server digest , return true</returns>
        internal bool CheckDigest(byte[] ping)
        {
            var reader = new MessagePackReader(ping);
            // check PING format
            if (!reader.TryReadArrayHeader(out var count) || count != 6)
            {
                return false;
            }
            // 1st value
            if (reader.ReadString() != "PING")
            {
                return false;
            }
            // 2nd value(use for calculate digest)
            var clientHostname = reader.ReadStringSequence();
            if (!clientHostname.HasValue)
            {
                return false;
            }
            // 3rd value(use for calculate digest)
            ReadOnlySequence<byte>? sharedKeySalt;
            switch (reader.NextCode)
            {
                case MessagePackCode.Str8:
                    sharedKeySalt = reader.ReadStringSequence();
                    break;
                case MessagePackCode.Bin8:
                case MessagePackCode.Bin16:
                case MessagePackCode.Bin32:
                case MessagePackCode.Str16:
                case MessagePackCode.Str32:
                    sharedKeySalt = reader.ReadBytes();
                    break;
                default:
                    if (reader.NextCode >= MessagePackCode.MinFixStr && reader.NextCode <= MessagePackCode.MaxFixStr)
                    {
                        sharedKeySalt = reader.ReadBytes();
                        break;
                    }

                    return false;
            }

            if (!sharedKeySalt.HasValue)
            {
                return false;
            }
            // 4th value
            var sharedKeyHexdigest = reader.ReadString();

            // 5th value
            // TODO: authorization(check username)
            if (reader.ReadString() != "")
            {
                return false;
            }
            // 6th value
            // TODO: authorization(check password)
            if (reader.ReadString() != "")
            {
                return false;
            }

            // verify digest
            using var memoryStream = new MemoryStream();
            memoryStream.Write(sharedKeySalt.Value.ToArray());
            memoryStream.Write(clientHostname.Value.ToArray());
            memoryStream.Write(Nonce!.FxToBytes());
            memoryStream.Write(SharedKey!.FxToBytes());
            memoryStream.Seek(0, SeekOrigin.Begin);
            using var algorithm = SHA512.Create();
            var hashBytes = algorithm.ComputeHash(memoryStream);

            var builder = new StringBuilder();
            foreach (var num in hashBytes)
                builder.Append(num.ToString("x2"));
            var digest = builder.ToString();

            return sharedKeyHexdigest == digest;
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