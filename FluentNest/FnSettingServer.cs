using System;

namespace FluentNest
{
    public class FnSettingServer
    {
        public string BindHost { get; set; } = "0.0.0.0";
        
        public int BindPort { get; set; } = 8710;
        
        public int TcpBackLog { get; set; } = 1024;

        public int TcpDivide { get; set; } = 2;

        public int TcpReadBufferSize { get; set; } = 1024 * 1024;

        public int TcpMaxStoredCount { get; set; } = 10;

        public int UdpDivide { get; set; } = 2;

        public int UdpReadBufferSize { get; set; } = 32;
    }
}