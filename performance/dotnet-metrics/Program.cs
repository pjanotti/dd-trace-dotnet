using System;
using StatsdClient;

namespace dotnet_metrics
{
    public class Program
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

            var statsd = new DogStatsdService();

            statsd.Configure(new StatsdConfig
                             {
                                 StatsdServerName = "localhost",
                                 Prefix = "dotnet-counters-",
                             });

            var counterSink = new StatsdCounterSink(statsd);
            var monitor = new CounterMonitor(processId, args[1], counterSink);
            monitor.Start();
        }
    }
}
