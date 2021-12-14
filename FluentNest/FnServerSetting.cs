namespace FluentNest
{
    public class FnServerSetting
    {
        public int TcpBackLog { get; set; } = 1024;

        public int TcpDivide { get; set; } = 3;

        public int TcpReadBufferSize { get; set; } = 8192;

        public int UdpDivide { get; set; } = 3;

        public int UdpReadBufferSize { get; set; } = 32;
    }
}