using System;
using OrangeCabinet;
using PurpleSofa;

namespace FluentNest
{
    public class FnServer
    {
        private PsServer? _tcpServer;

        private OcLocal? _udpServer;

        public string BindHost { get; set; } = "0.0.0.0";
        
        public int BindPort { get; set; } = 24224;

        public FnServerSetting? ServerSetting { get; set; }
        
        public FnClientSetting? ClientSetting { get; set; }

        public void Start(IFnCallback callback)
        {
            // server setting
            ServerSetting ??= new FnServerSetting();
            
            // config
            ClientSetting ??= new FnClientSetting();

            // tcp server
            _tcpServer ??= new PsServer(new FnTcpCallback(ClientSetting, callback))
            {
                Host = BindHost,
                Port = BindPort,
                Backlog = ServerSetting.TcpBackLog,
                Divide = ServerSetting.TcpDivide,
                ReadBufferSize = ServerSetting.TcpReadBufferSize
            };
            _tcpServer.Start();
            
            // udp server
            _udpServer ??= new OcLocal(new OcBinder(new FnUdpCallback(ClientSetting))
            {
                BindHost = BindHost,
                BindPort = BindPort,
                Divide = ServerSetting.UdpDivide,
                ReadBufferSize = ServerSetting.UdpReadBufferSize
            });
            _udpServer.Start();
        }
    }

    internal class FnTcpCallback : PsCallback
    {
        private FnClientSetting _clientSetting;

        private IFnCallback _callback;
        
        public FnTcpCallback(FnClientSetting clientSetting, IFnCallback callback)
        {
            _clientSetting = clientSetting;
            _callback = callback;
        }

        public override void OnOpen(PsSession session)
        {
            // timeout
            session.ChangeIdleMilliSeconds(_clientSetting.TcpTimeout * 1000);
            
            // handshake - send HELO
        }

        public override void OnMessage(PsSession session, byte[] message)
        {
            // handshake - receive PING
            
            // handshake - send PONG
            
            // receive message
            try
            {

            }
            catch (Exception e)
            {
                
            }
            
            // send ack
            // TODO if contains chunk option.
        }
    }

    internal class FnUdpCallback : OcCallback
    {
        private FnClientSetting _clientSetting;
        
        public FnUdpCallback(FnClientSetting clientSetting)
        {
            _clientSetting = clientSetting;
        }

        public override void Incoming(OcRemote remote, byte[] message)
        {
            // timeout
            remote.ChangeIdleMilliSeconds(_clientSetting.UdpTimeout * 1000);
            
            // heartbeat - receive MAY
            
            // heartbeat - send SHOULD
        }
    }
}