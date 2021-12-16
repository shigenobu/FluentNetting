namespace FluentNest
{
    public class FnServerSetting
    {
        public string BindHost { get; set; } = "0.0.0.0";
        
        public int BindPort { get; set; } = 8710;
        
        public int TcpBackLog { get; set; } = 1024;

        public int TcpDivide { get; set; } = 3;

        public int TcpReadBufferSize { get; set; } = 8192;

        public int UdpDivide { get; set; } = 3;

        public int UdpReadBufferSize { get; set; } = 32;
    }
}