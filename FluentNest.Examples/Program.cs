using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Serilog;
using Serilog.Sinks.Fluentd;

namespace FluentNest.Examples
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var server = new FnServer(new ExampleCallback())
            {
                Config = new FnConfig(),
                SettingClient = new FnSettingClient(),
                SettingServer = new FnSettingServer()
            };
            server.Start();

            // forward to server (and will be received)
            var timer = new Timer(3000);
            timer.Elapsed += async (sender, e) => await HandleTimer();
            timer.Start();

            server.WaitFor();
        }

        private static Task HandleTimer()
        {
            // send to fluentd or fluent-bit in localhost
            var options = new FluentdSinkOptions("localhost", 24224, "tag.example");
            var log = new LoggerConfiguration().WriteTo.Fluentd(options).CreateLogger();
            log.Information("hello {0}!", "world");
            return Task.CompletedTask;
        }
    }

    public class ExampleCallback : IFnCallback
    {
        public void Receive(string tag, List<FnMessageEntry> entries)
        {
            Console.WriteLine($"tag:{tag}, entries:[{string.Join(", ", entries.Select(e => $"{{{e}}}"))}]");
        }
    }
}