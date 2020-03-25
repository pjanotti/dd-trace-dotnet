using System;
using StatsdClient;

namespace dotnet_metrics
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int processId) || string.IsNullOrWhiteSpace(args[1]))
            {
                Console.WriteLine("Usage: dotnet-metrics.exe <pid> <name>");
                return;
            }

            Console.WriteLine($"Subscribing to counters on process {processId}.");
            Console.WriteLine("Press CTRL+C to exit...");

            var statsdUdp = new StatsdUDP("localhost", StatsdConfig.DefaultStatsdPort);
            var statsd = new Statsd(statsdUdp, "dotnet-counters-");
            var statsdShipper = new CounterDataStatsdShipper(statsd);

            var consoleShipper = new ConsoleShipper();

            var monitor = new CounterMonitor(processId, args[1]);
            monitor.Subscribe(statsdShipper);
            monitor.Subscribe(consoleShipper);
            monitor.Start();
        }
    }
}
