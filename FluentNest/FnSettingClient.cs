namespace FluentNest
{
    /// <summary>
    ///     Setting client.
    /// </summary>
    public class FnSettingClient
    {
        /// <summary>
        ///     Tcp timeout, default 65seconds.
        ///     On the condition, fluentd flush interval 60seconds under.
        /// </summary>
        public int TcpTimeout { get; set; } = 65;
        
        /// <summary>
        ///     Udp timeout, default 15.
        ///     On the condition, fluentd heartbeat interval 10seconds under.
        /// </summary>
        public int UdpTimeout { get; set; } = 15;
    }
}