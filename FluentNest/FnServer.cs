using System;
using System.IO;
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

        public FnSettingServer? SettingServer { get; set; }
        
        public FnSettingClient? SettingClient { get; set; }

        public FnServer(IFnCallback callback)
        {
            _callback = callback;
        }
        
        public void Start()
        {
            // server setting
            SettingServer ??= new FnSettingServer();
            
            // config
            SettingClient ??= new FnSettingClient();

            // tcp server
            _tcpServer ??= new PsServer(new FnTcpCallback(SettingServer, SettingClient, _callback))
            {
                Host = SettingServer.BindHost,
                Port = SettingServer.BindPort,
                Backlog = SettingServer.TcpBackLog,
                Divide = SettingServer.TcpDivide,
                ReadBufferSize = SettingServer.TcpReadBufferSize
            };
            _tcpServer.Start();
            
            // udp server
            _udpServer ??= new OcLocal(new OcBinder(new FnUdpCallback(SettingClient))
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
        
        private readonly FnSettingServer _settingServer;
        private readonly FnSettingClient _settingClient;

        private readonly IFnCallback _callback;
        
        public FnTcpCallback(FnSettingServer settingServer, FnSettingClient settingClient, IFnCallback callback)
        {
            _settingServer = settingServer;
            _settingClient = settingClient;
            _callback = callback;
        }

        public override void OnOpen(PsSession session)
        {
            // timeout
            session.ChangeIdleMilliSeconds(_settingClient.TcpTimeout * 1000);
            
            // TODO handshake - send HELO
            // var helo = new FnMessageHelo();
            // helo.Option.Nonce = "8IBTHwOdqNKAWeKl7plt8g==";
            // helo.Option.Keepalive = false;
            // Console.WriteLine(MessagePackSerializer.SerializeToJson(helo));
            // session.Send(MessagePackSerializer.Serialize(helo));
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
            
            // TODO handshake - receive PING
            
            // TODO handshake - send PONG
            
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
            try
            {
                // receive
                _callback.Receive(msg!);
            }
            catch (Exception e)
            {
                FnLogger.Error(e);

                // error
                _callback.Error(e, msg!);
            }
            
            // send ack
            if (_settingClient.RequireAck && msg is {Option: {Chunk: { }}})
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
        private readonly FnSettingClient _settingClient;
        
        public FnUdpCallback(FnSettingClient settingClient)
        {
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