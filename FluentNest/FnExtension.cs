using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using OrangeCabinet;

namespace FluentNest
{
    internal static class FnExtension
    {
        internal static byte[] FxConcat(this byte[] self, byte[] additional)
        {
            var output = new byte[self.Length + additional.Length];
            for (var i = 0; i < self.Length; i++)
                output[i] = self[i];
            for (var j = 0; j < additional.Length; j++)
                output[self.Length + j] = additional[j];
            return output;    
        }
        
        /// <summary>
        ///     Byte[] to utf8 string.
        /// </summary>
        /// <param name="self">byte array</param>
        /// <returns>utf8 string</returns>
        /// <exception cref="OcExtensionException">error</exception>
        internal static string FxToString(this byte[] self)
        {
            try
            {
                return Encoding.UTF8.GetString(self);
            }
            catch (Exception e)
            {
                FnLogger.Error(e);
                throw new FnExtensionException(e);
            }
        }
        
        /// <summary>
        ///     Utf8 string to byte array.
        /// </summary>
        /// <param name="self">utf8 string</param>
        /// <returns>byte array</returns>
        /// <exception cref="OcExtensionException">error</exception>
        internal static byte[] FxToBytes(this string self)
        {
            try
            {
                return Encoding.UTF8.GetBytes(self);
            }
            catch (Exception e)
            {
                FnLogger.Error(e);
                throw new FnExtensionException(e);
            }
        }

        internal static string FxToHexString(this IEnumerable<byte> self)
        {
            var builder = new StringBuilder();
            var sep = string.Empty;
            foreach (var b in self.ToArray())
            {
                builder.Append(sep);
                builder.Append(b.ToString("X"));
                sep = " ";
            }

            return builder.ToString();
        }

        internal static string FxSha512(this string src)
        {
            var data = Encoding.UTF8.GetBytes(src);
            try
            {
                using var algorithm = SHA512.Create();
                var hashBytes = algorithm.ComputeHash(data);
 
                var builder = new StringBuilder();
                foreach (var num in hashBytes)
                    builder.Append(num.ToString("x2"));
 
                return builder.ToString();
            }
            catch (Exception e)
            {
                FnLogger.Error(e);
                throw new FnExtensionException(e);
            }
        }

        /// <summary>
        ///     Extension exception.
        /// </summary>
        public class FnExtensionException : Exception
        {
            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="exception">error</param>
            internal FnExtensionException(Exception exception) : base(exception.ToString())
            {
            }
        }
    }
}