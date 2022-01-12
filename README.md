# FluentNest - fluentd / fluent-bit forwarded server

### feature

'FluentNest' is fluent forwarding message received server that is based on [Fluent forward protocol v1 specification](https://github.com/fluent/fluentd/wiki/Forward-Protocol-Specification-v1).  
This library is supported for both fluentd and fluent-bit.  

### supported message mode (format)

* Message Mode
* Forward Mode
* PackedForward Mode
* CompressedPackedForward Mode

### supported configuration  

* Security forwarding authorization, `HELO`, `PING` and `PONG` (not tls, not username/password, only self_hostname/shared_key).
* Udp heartbeat.

---

### how to use

(data flow)  

    client ---(forward)---> fluend or fluent-bit ---(forward)---> server used by FluentNest

##### callback

    public class ExampleCallback : IFnCallback
    {
        public void Receive(string tag, List<FnMessageEntry> entries)
        {
            Console.WriteLine($"tag:{tag}, entries:[{string.Join(", ", entries.Select(e => $"{{{e}}}"))}]");
        }
    }

##### server

    var server = new FnServer(new ExampleCallback())
    {
        // default - not authrization, enable 'RequireAck', disable 'Keepalive'
        Config = new FnConfig(),
        // default - tcp timeout 65sec, udp timeout 15sec
        SettingClient = new FnSettingClient(),
        // default - listening on tcp://0.0.0.0:8710, udp://0.0.0.0:8710
        SettingServer = new FnSettingServer()
    };
    server.Start();
    server.WaitFor();

##### fluent configuration (fluentd or fluent-bit in a same host)

(fluentd)  

    <source>
      @type forward
      port 24224
    </source>
    
    <match **>
      @type forward  
      send_timeout 60s
      recover_wait 10s
      heartbeat_type udp
      heartbeat_interval 5s
      phi_threshold 16
      hard_timeout 60s
      require_ack_response
    
      <server>
        host {{ your server address }}
        port 8710
      </server>
    
      buffer_type file
      buffer_path /fluentd/log/buffer
      buffer_chunk_limit 1m
      retry_limit 3
      flush_interval 1m
    </match>

(fluent-bit)  

    [SERVICE]
        Flush         5
        Daemon        off
        Log_Level     info
        storage.path  /fluent-bit/log

    [INPUT]
        Name               forward
        Listen             0.0.0.0
        Port               24224
        storage.type       filesystem
        Buffer_Chunk_Size  1M
        Buffer_Max_Size    6M

    [OUTPUT]
        Name                  forward
        Match                 *
        Host                  {{ your server address }}
        Port                  8710
        Require_ack_response  true
        Send_options          true

##### client - csharp used by Serilog

    var options = new FluentdSinkOptions("your fluentd or fluent-bit server address", 24224, "tag.example");
    var log = new LoggerConfiguration().WriteTo.Fluentd(options).CreateLogger();
    log.Information("hello {0}!", "world");

In detail, confirm [example](FluentNest.Examples/Program.cs).  

---

### motivated and referenced

* [influent](https://github.com/okumin/influent) - java fluentd forward server.
* [MessagePack-CSharp](https://github.com/neuecc/MessagePack-CSharp) - csharp fastest message pack parser.
