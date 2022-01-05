using System;
using System.Linq;
using MessagePack;
using OrangeCabinet;
using PurpleSofa;

namespace FluentNest
{
    /// <summary>
    ///     Server.
    /// </summary>
    public class FnServer
    {
        /// <summary>
        ///     Callback.
        /// </summary>
        private readonly IFnCallback _callback;

        /// <summary>
        ///     Tcp server.
        /// </summary>
        private PsServer? _tcpServer;

        /// <summary>
        ///     Udp server.
        ///     If not set, creating with default server & client setting.
        /// </summary>
        private OcLocal? _udpServer;

        /// <summary>
        ///     Config.
        ///     If not set, used by default config.
        /// </summary>
        public FnConfig? Config { get; set; }

        /// <summary>
        ///     Setting server.
        ///     If not set, used by default server setting.
        /// </summary>
        public FnSettingServer? SettingServer { get; set; }

        /// <summary>
        ///     Setting client.
        ///     If not set, used by default client setting.
        /// </summary>
        public FnSettingClient? SettingClient { get; set; }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="callback">callback</param>
        public FnServer(IFnCallback callback)
        {
            _callback = callback;
        }

        /// <summary>
        ///     Start.
        /// </summary>
        public void Start()
        {
            // config
            Config ??= new FnConfig();

            // server setting
            SettingServer ??= new FnSettingServer();

            // config
            SettingClient ??= new FnSettingClient();

            // tcp server
            _tcpServer ??= new PsServer(new FnTcpCallback(Config, SettingServer, SettingClient, _callback))
            {
                Host = SettingServer.BindHost,
                Port = SettingServer.BindPort,
                Backlog = SettingServer.TcpBackLog,
                Divide = SettingServer.TcpDivide,
                ReadBufferSize = SettingServer.TcpReadBufferSize
            };
            _tcpServer.Start();

            // udp server
            _udpServer ??= new OcLocal(new OcBinder(new FnUdpCallback(Config, SettingClient))
            {
                BindHost = SettingServer.BindHost,
                BindPort = SettingServer.BindPort,
                Divide = SettingServer.UdpDivide,
                ReadBufferSize = SettingServer.UdpReadBufferSize
            });
            _udpServer.Start();
        }

        /// <summary>
        ///     Wait for.
        /// </summary>
        public void WaitFor()
        {
            _tcpServer?.WaitFor();
            _udpServer?.WaitFor();
        }

        /// <summary>
        ///     Shutdown.
        /// </summary>
        public void Shutdown()
        {
            _udpServer?.Shutdown();
            _tcpServer?.Shutdown();
        }
    }

    /// <summary>
    ///     Tcp callback.
    /// </summary>
    internal class FnTcpCallback : PsCallback
    {
        /// <summary>
        ///     Stored key for tcp divide message.
        /// </summary>
        private const string TmpStoredKey = "__tmpStoredKey";

        /// <summary>
        ///     Stored key for tcp divide count of message.
        /// </summary>
        private const string TmpStoredCount = "__tmpStoredCount";

        /// <summary>
        ///     Authorized key, if 'security' section used.
        /// </summary>
        private const string AuthorizedKey = "__authorizedKey";

        /// <summary>
        ///     Config.
        /// </summary>
        private readonly FnConfig _config;

        /// <summary>
        ///     Setting server.
        /// </summary>
        private readonly FnSettingServer _settingServer;

        /// <summary>
        ///     Setting client.
        /// </summary>
        private readonly FnSettingClient _settingClient;

        /// <summary>
        ///     Callback.
        /// </summary>
        private readonly IFnCallback _callback;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="config">config</param>
        /// <param name="settingServer">setting server</param>
        /// <param name="settingClient">setting client</param>
        /// <param name="callback">callback</param>
        internal FnTcpCallback(FnConfig config, FnSettingServer settingServer, FnSettingClient settingClient,
            IFnCallback callback)
        {
            _config = config;
            _settingServer = settingServer;
            _settingClient = settingClient;
            _callback = callback;
        }

        /// <summary>
        ///     On open.
        /// </summary>
        /// <param name="session">session</param>
        public override void OnOpen(PsSession session)
        {
            // timeout
            session.ChangeIdleMilliSeconds(_settingClient.TcpTimeout * 1000);

            // authorization
            if (_config.EnableAuthorization())
            {
                // handshake - send HELO
                // var helo = new FnMsgpackOutHelo();
                // helo.Option.Nonce = _config.Nonce!;
                // helo.Option.Keepalive = _config.KeepAlive;
                var helo = new FnMsgpackOutHelo2(_config.Nonce!, null, _config.KeepAlive);
                FnLogger.Debug(() => $"Send 'HELO': {MessagePackSerializer.SerializeToJson(helo)}");
                session.Send(MessagePackSerializer.Serialize(helo));
            }
        }

        /// <summary>
        ///     On message.
        /// </summary>
        /// <param name="session">session</param>
        /// <param name="message">message</param>
        public override void OnMessage(PsSession session, byte[] message)
        {
            // get stored count from session
            int storedCount = session.GetValue<int>(TmpStoredCount);

            // get stored message from session
            var newMessage = session.GetValue<byte[]>(TmpStoredKey);
            if (newMessage != null)
            {
                message = newMessage.FxConcat(message);
            }

            // authorization
            if (_config.EnableAuthorization())
            {
                var authorized = session.GetValue<bool>(AuthorizedKey);
                if (!authorized)
                {
                    try
                    {
                        // handshake - receive PING
                        var ping = MessagePackSerializer.Deserialize<FnMsgpackInPing>(message);
                        FnLogger.Debug(() => $"Read 'PING': {MessagePackSerializer.SerializeToJson(ping)}");

                        // clear session
                        session.ClearValue(TmpStoredKey);
                        session.ClearValue(TmpStoredCount);

                        // check digest
                        var authResult = true;
                        var reason = string.Empty;
                        if (!_config.CheckDigest(message))
                        {
                            authResult = false;
                            reason = "Illegal";
                        }

                        session.SetValue(AuthorizedKey, authResult);

                        // handshake - send PONG
                        var pong = new FnMsgpackOutPong()
                        {
                            AuthResult = authResult,
                            ServerHostname = _config.ServerHostname,
                            Reason = reason,
                            SharedKeyHexdigest = _config.CreateDigest(ping)
                        };
                        session.Send(MessagePackSerializer.Serialize(pong));
                        FnLogger.Debug(() => $"Send 'PONG': {MessagePackSerializer.SerializeToJson(pong)}");

                        // handshake - keep connection or disconnect
                        if (!authResult)
                        {
                            session.Close();
                        }
                    }
                    catch (Exception e)
                    {
                        FnLogger.Debug(e);

                        // stored to session
                        // session.SetValue(TmpStoredKey, message);
                        // session.SetValue(TmpStoredCount, ++storedCount);

                        session.Close();
                    }

                    return;
                }
            }

            // receive message
            if (!FnMsgpackParser.TryParse(message, out var msg))
            {
                // stored to session
                session.SetValue(TmpStoredKey, message);
                session.SetValue(TmpStoredCount, ++storedCount);

                // clear session
                if (storedCount > _settingServer.TcpMaxStoredCount)
                {
                    session.ClearValue(TmpStoredKey);
                    session.ClearValue(TmpStoredCount);
                }

                return;
            }

            // clear session
            session.ClearValue(TmpStoredKey);
            session.ClearValue(TmpStoredCount);

            // callback
            if (msg!.Entries != null && msg.Entries.Any())
            {
                try
                {
                    // receive
                    _callback.Receive(msg.Tag, msg.Entries);
                }
                catch (Exception e)
                {
                    FnLogger.Error(e);
                }
            }

            // send ack
            if (_config.RequireAck && msg is { Option: { Chunk: { } } })
            {
                try
                {
                    var res = new FnMsgpackOutAck
                    {
                        Ack = msg.Option.Chunk
                    };
                    session.Send(MessagePackSerializer.Serialize(res));
                    FnLogger.Debug(() => $"Ack:{res.Ack}");
                }
                catch (Exception e)
                {
                    FnLogger.Error(e);
                }
            }
        }

        /// <summary>
        ///     On close.
        /// </summary>
        /// <param name="session">session</param>
        /// <param name="closeReason">close reason</param>
        public override void OnClose(PsSession session, PsCloseReason closeReason)
        {
            FnLogger.Debug(() => $"OnClose session:{session}, closeReason:{closeReason}");
        }
    }

    /// <summary>
    ///     Udp callback.
    /// </summary>
    internal class FnUdpCallback : OcCallback
    {
        /// <summary>
        ///     Config.
        /// </summary>
        private readonly FnConfig _config;

        /// <summary>
        ///     Setting client.
        /// </summary>
        private readonly FnSettingClient _settingClient;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="config">config</param>
        /// <param name="settingClient">setting client</param>
        internal FnUdpCallback(FnConfig config, FnSettingClient settingClient)
        {
            _config = config;
            _settingClient = settingClient;
        }

        /// <summary>
        ///     Incoming.
        /// </summary>
        /// <param name="remote">remote</param>
        /// <param name="message">message</param>
        public override void Incoming(OcRemote remote, byte[] message)
        {
            // timeout
            remote.ChangeIdleMilliSeconds(_settingClient.UdpTimeout * 1000);

            // heartbeat - receive MAY
            if (message.Length == 0 || message[0] != 0x00)
            {
                FnLogger.Debug(() => $"Illegal heartbeat.");
                return;
            }

            // heartbeat - send SHOULD
            remote.Send(message);
        }
    }
}