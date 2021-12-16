using System;
using System.IO;
using MessagePack;
using OrangeCabinet;
using PurpleSofa;

namespace FluentNest
{
    public class FnServer
    {
        private IFnCallback _callback;
        
        private PsServer? _tcpServer;

        private OcLocal? _udpServer;

        public FnServerSetting? ServerSetting { get; set; }
        
        public FnClientSetting? ClientSetting { get; set; }

        public FnServer(IFnCallback callback)
        {
            _callback = callback;
        }
        
        public void Start()
        {
            // server setting
            ServerSetting ??= new FnServerSetting();
            
            // config
            ClientSetting ??= new FnClientSetting();

            // tcp server
            _tcpServer ??= new PsServer(new FnTcpCallback(ClientSetting, _callback))
            {
                Host = ServerSetting.BindHost,
                Port = ServerSetting.BindPort,
                Backlog = ServerSetting.TcpBackLog,
                Divide = ServerSetting.TcpDivide,
                ReadBufferSize = ServerSetting.TcpReadBufferSize
            };
            _tcpServer.Start();
            
            // udp server
            _udpServer ??= new OcLocal(new OcBinder(new FnUdpCallback(ClientSetting))
            {
                BindHost = ServerSetting.BindHost,
                BindPort = ServerSetting.BindPort,
                Divide = ServerSetting.UdpDivide,
                ReadBufferSize = ServerSetting.UdpReadBufferSize
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
            _tcpServer?.Shutdown();
            _udpServer?.Shutdown();
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
            // var helo = new FnMessageHelo();
            // helo.Option.Nonce = "8IBTHwOdqNKAWeKl7plt8g==";
            // helo.Option.Keepalive = false;
            // Console.WriteLine(MessagePackSerializer.SerializeToJson(helo));
            // session.Send(MessagePackSerializer.Serialize(helo));
        }

        public override void OnMessage(PsSession session, byte[] message)
        {
            File.WriteAllBytes("/home/furuta/02-development/rider/FluentNest/FluentNest/data.bin", message);
            
            Console.WriteLine(message.PxToString());
            var msg = MessagePackSerializer.Deserialize<FnMessageForward>(message);
            Console.WriteLine($"{MessagePackSerializer.SerializeToJson(msg)}");


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
            remote.Send(message);
        }
    }
}