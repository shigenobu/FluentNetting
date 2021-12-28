namespace FluentNest
{
    /// <summary>
    ///     Setting server.
    /// </summary>
    public class FnSettingServer
    {
        /// <summary>
        ///     Bind host, default 0.0.0.0, tcp and udp.
        /// </summary>
        public string BindHost { get; set; } = "0.0.0.0";
        
        /// <summary>
        ///     Bind port, default 8710, tcp and udp.
        /// </summary>
        public int BindPort { get; set; } = 8710;
        
        /// <summary>
        ///     Tcp backlog, default 1024.
        /// </summary>
        public int TcpBackLog { get; set; } = 1024;

        /// <summary>
        ///     Tcp manage session divide, default 2.
        /// </summary>
        public int TcpDivide { get; set; } = 2;

        /// <summary>
        ///     Tcp read buffer size, default 1MiB.
        /// </summary>
        public int TcpReadBufferSize { get; set; } = 1024 * 1024;

        /// <summary>
        ///     Tcp max stored count, default 10.
        /// </summary>
        public int TcpMaxStoredCount { get; set; } = 10;

        /// <summary>
        ///     Udp manage remote divide, default 2.
        /// </summary>
        public int UdpDivide { get; set; } = 2;

        /// <summary>
        ///     Udp read buffer size, default 32Byte.
        /// </summary>
        public int UdpReadBufferSize { get; set; } = 32;
    }
}