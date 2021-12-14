namespace FluentNest
{
    public class FnConfig
    {
        // public FnConfigForwardMode ForwardMode { get; set; }
        //
        // public FnConfigHeartBeatType HeartBeatType { get; set; }

        public int TcpTimeout { get; set; } = 65;
        
        public int UdpTimeout { get; set; } = 15;
    }

    public enum FnConfigForwardMode
    {
        Forward = default,
        SecureForward,
    }
    
    public enum FnConfigHeartBeatType
    {
        Udp = default,
        Tcp
    }
}