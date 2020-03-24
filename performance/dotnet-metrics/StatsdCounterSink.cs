using System;
using StatsdClient;

namespace dotnet_metrics
{
    public class StatsdCounterSink : ICounterSink
    {
        private readonly DogStatsdService _statsd;

        public StatsdCounterSink(DogStatsdService statsd)
        {
            _statsd = statsd ?? throw new ArgumentNullException(nameof(statsd));
        }

        public void OnCounterUpdate(CounterEventArgs counterEventArgs)
        {
            // Console.WriteLine($"{counterEventArgs.ProcessId}\t{counterEventArgs.Counter}\t{counterEventArgs.CounterDisplayName}\t{counterEventArgs.Value}");

            string[] tags =
            {
                $"process-id:{counterEventArgs.ProcessId}",
                $"process-name:{counterEventArgs.ProcessName}"
            };

            switch (counterEventArgs.Type)
            {
                case CounterType.Sum:
                    _statsd.Counter(counterEventArgs.CounterName, counterEventArgs.Value, tags: tags);
                    break;
                case CounterType.Mean:
                    _statsd.Gauge(counterEventArgs.CounterName, counterEventArgs.Value, tags: tags);
                    break;
            }
        }
    }
}
