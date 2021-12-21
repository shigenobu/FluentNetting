using System;
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
            
            FnLogger.Verbose = true;
            FnLogger.Transfer = (msg) => outputHelper.WriteLine(msg?.ToString());
        }
        
        [Fact]
        public void TestForever()
        {
            var server = new FnServer(new TestCallback())
            {
                SettingClient = new FnSettingClient(),
                SettingServer = new FnSettingServer()
            };
            server.Start();
            server.WaitFor();
        }
    }

    public class TestCallback : IFnCallback
    {
        public void Receive(FnMessage msg)
        {
            FnLogger.Debug(msg);
        }

        public void Error(Exception e, FnMessage msg)
        {
            FnLogger.Error(e);
        }
    }
}