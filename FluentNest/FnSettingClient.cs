namespace FluentNest
{
    public class FnSettingClient
    {
        public int TcpTimeout { get; set; } = 65;
        
        public int UdpTimeout { get; set; } = 15;

        public bool RequireAck { get; set; } = true;
    }
}