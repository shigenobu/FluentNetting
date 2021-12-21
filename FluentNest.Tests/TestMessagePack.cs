using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace FluentNest.Tests
{
    public class TestMessagePack
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TestMessagePack(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;

            FnLogger.Verbose = true;
            FnLogger.Transfer = (msg) => _testOutputHelper.WriteLine(msg?.ToString());
        }

        [Fact]
        public void TestFowardError()
        {
            var data = File.ReadAllBytes("data/error.bin");

            var msg = new FnMsgpackParser().Unpack(data);
            _testOutputHelper.WriteLine(msg.ToString());
        }
    }
}