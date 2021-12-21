using System.Collections.Generic;
using System.Formats.Asn1;
using OrangeCabinet;
using PurpleSofa;
using Xunit;
using Xunit.Abstractions;

namespace FluentNest.Tests
{
    public class TestServer
    {
        public TestServer(ITestOutputHelper outputHelper)
        {
            OcLogger.Verbose = true;
            PsLogger.Verbose = true;
        }
        
        [Fact]
        public void TestForever()
        {
            var server = new FnServer(new TestCallback());
            server.Start();
            server.WaitFor();
        }
    }

    public class TestCallback : IFnCallback
    {
        public void Receive(FnMessage msg)
        {
            
        }
    }
}