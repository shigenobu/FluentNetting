using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace FluentNest.Tests
{
    public class TestAuthorization
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TestAuthorization(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void TestConfirm()
        {
            var nonce = "abc";
            var sharedKey = "0123456789";
            var hostname = "fn-fluent01";
            var salt = "9cbed8560b0e47a64029e2239ce11054";
            var hexDigest =
                "b70c88961baeba324befc885c2df73225a2e4247425e695b3027eec4725b6629a212e8324c2069dc1db473d4d1b74a7be5db0295eecaee483d9a1cc03d072588";

            var srcStr = $"{salt}{hostname}{nonce}{sharedKey}";
            _testOutputHelper.WriteLine(srcStr);
            // var srcData = StringToBytes(srcStr);

            var hash = GetSha512(Encoding.UTF8.GetBytes(srcStr));
            _testOutputHelper.WriteLine(hash);
            
            Assert.Equal(hexDigest, hash);
        }
        
        public static byte[] StringToBytes(string str)
        {
            var bs = new List<byte>();
            for (int i = 0; i < str.Length / 2; i++)
            {
                bs.Add(Convert.ToByte(str.Substring(i*2, 2)));
            }
            // "01-AB-EF" こういう"-"区切りを想定する場合は以下のようにする
            // var bs = str.Split('-').Select(hex => Convert.ToByte(hex, 16));
            return bs.ToArray();
        }
        
        private string GetSha512(byte[] src)
        {
            try
            {
                using var algorithm = SHA512.Create();
                var hashBytes = algorithm.ComputeHash(src);
 
                var builder = new StringBuilder();
                foreach (var num in hashBytes)
                    builder.Append(num.ToString("x2"));
 
                return builder.ToString();
            }
            catch (Exception e)
            {
            }

            return string.Empty;
        }

    }
}