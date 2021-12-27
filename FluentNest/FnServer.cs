using System;
using System.IO;
using System.Linq;
using MessagePack;
using OrangeCabinet;
using PurpleSofa;

namespace FluentNest
{
    public class FnServer
    {
        private readonly IFnCallback _callback;
        
        private PsServer? _tcpServer;

        private OcLocal? _udpServer;

        public FnConfig? Config { get; set; }
        
        public FnSettingServer? SettingServer { get; set; }
        
        public FnSettingClient? SettingClient { get; set; }

        public FnServer(IFnCallback callback)
        {
            _callback = callback;
        }
        
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
        
        public void WaitFor()
        {
            _tcpServer?.WaitFor();
            _udpServer?.WaitFor();
        }

        public void Shutdown()
        {
            _udpServer?.Shutdown();
            _tcpServer?.Shutdown();
        }
    }

    internal class FnTcpCallback : PsCallback
    {
        private const string TmpStoredKey = "__tmpStoredKey";
        private const string TmpStoredCount = "__tmpStoredCount";
        private const string AuthorizedKey = "__authorizedKey";

        private readonly FnConfig _config;
        private readonly FnSettingServer _settingServer;
        private readonly FnSettingClient _settingClient;

        private readonly IFnCallback _callback;
        
        public FnTcpCallback(FnConfig config, FnSettingServer settingServer, FnSettingClient settingClient,
            IFnCallback callback)
        {
            _config = config;
            _settingServer = settingServer;
            _settingClient = settingClient;
            _callback = callback;
        }

        public override void OnOpen(PsSession session)
        {
            // timeout
            session.ChangeIdleMilliSeconds(_settingClient.TcpTimeout * 1000);
            
            // authorization
            if (_config.EnableAuthorization())
            {
                // handshake - send HELO
                var helo = new FnMsgpackOutHelo();
                helo.Option.Nonce = _config.Nonce!;
                helo.Option.Keepalive = _config.KeepAlive;
                FnLogger.Debug(() => $"Send 'HELO': {MessagePackSerializer.SerializeToJson(helo)}");
                session.Send(MessagePackSerializer.Serialize(helo));    
            }
        }

        public override void OnMessage(PsSession session, byte[] message)
        {
            // get stored count from session
            int storedCount = session.GetValue<int>(TmpStoredCount);

            // TODO divided message what to do ?
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
                        if (!_config.CheckDigest(ping))
                        {
                            authResult = false;
                            reason = "Illegal";
                        }
                        session.SetValue(AuthorizedKey, authResult);

                        // handshake - send PONG
                        var hash = $"{ping.ShareKeySalt}{_config.ServerHostname}{_config.Nonce}{_config.SharedKey}";
                        var pong = new FnMsgpackOutPong()
                        {
                            AuthResult = authResult,
                            ServerHostname = _config.ServerHostname,
                            Reason = reason,
                            SharedKeyHexdigest = hash.FxSha512()
                        };
                        session.Send(MessagePackSerializer.Serialize(pong));
                        FnLogger.Debug(() => $"Send 'PONG': {MessagePackSerializer.SerializeToJson(pong)}");
                    }
                    catch (Exception e)
                    {
                        FnLogger.Debug(e);
                        
                        // stored to session
                        session.SetValue(TmpStoredKey, message);
                        session.SetValue(TmpStoredCount, ++storedCount);
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
            if (_config.RequireAck && msg is {Option: {Chunk: { }}})
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

        public override void OnClose(PsSession session, PsCloseReason closeReason)
        {
            FnLogger.Debug(() => $"OnClose session:{session}, closeReason:{closeReason}");
        }
    }

    internal class FnUdpCallback : OcCallback
    {
        private readonly FnConfig _config;
        private readonly FnSettingClient _settingClient;
        
        public FnUdpCallback(FnConfig config, FnSettingClient settingClient)
        {
            _config = config;
            _settingClient = settingClient;
        }

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