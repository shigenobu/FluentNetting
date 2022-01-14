using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using OrangeCabinet;
using PurpleSofa;
using Xunit;
using Xunit.Abstractions;

namespace FluentNetting.Tests
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
            // forward
            var server = new FnServer(new TestCallback())
            {
                Config = new FnConfig(),
                SettingClient = new FnSettingClient(),
                SettingServer = new FnSettingServer
                {
                    BindHost = "0.0.0.0",
                    BindPort = 8710
                }
            };
            server.Start();
            
            // secure forward
            var serverSecure = new FnServer(new TestCallback())
            {
                Config = new FnConfig
                {
                    Nonce = "ABC",
                    SharedKey = "0123456789",
                    KeepAlive = true
                },
                SettingClient = new FnSettingClient(),
                SettingServer = new FnSettingServer
                {
                    BindHost = "0.0.0.0",
                    BindPort = 8711
                }
            };
            serverSecure.Start();
            
            server.WaitFor();
            serverSecure.WaitFor();
        }
    }

    public class TestCallback : IFnCallback
    {
        public void Receive(string tag, List<FnMessageEntry> entries)
        {
            FnLogger.Debug($"tag:{tag}, entries:[{string.Join(", ", entries.Select(e => e.ToString()))}]");
        }
    }
}