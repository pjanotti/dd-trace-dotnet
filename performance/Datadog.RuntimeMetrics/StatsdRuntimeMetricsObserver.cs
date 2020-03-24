using System;
using StatsdClient;

namespace Datadog.RuntimeMetrics
{
    public class StatsdRuntimeMetricsObserver : IObserver<RuntimeMetrics>
    {
        private const double SampleRate = 1d;

        private readonly IStatsd _statsd;

        private int _lastGen0Count;
        private int _lastGen1Count;
        private int _lastGen2Count;

        public StatsdRuntimeMetricsObserver(IStatsd statsd)
        {
            _statsd = statsd ?? throw new ArgumentNullException(nameof(statsd));
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(RuntimeMetrics value)
        {
            int gen0Diff = value.Gen0 - _lastGen0Count;
            _lastGen0Count = value.Gen0;

            int gen1Diff = value.Gen1 - _lastGen1Count;
            _lastGen1Count = value.Gen1;

            int gen2Diff = value.Gen2 - _lastGen2Count;
            _lastGen2Count = value.Gen2;

            _statsd.Add<Statsd.Gauge, long>("dotnet_counters.gc_heap_size", value.Allocated, SampleRate);
            _statsd.Add<Statsd.Gauge, long>("dotnet_counters.working_set", value.WorkingSet, SampleRate);
            _statsd.Add<Statsd.Gauge, long>("dotnet_counters.private_bytes", value.PrivateBytes, SampleRate);
            _statsd.Add<Statsd.Counting, int>("dotnet_counters.gen_0_gc_count", gen0Diff, SampleRate);
            _statsd.Add<Statsd.Counting, int>("dotnet_counters.gen_1_gc_count", gen1Diff, SampleRate);
            _statsd.Add<Statsd.Counting, int>("dotnet_counters.gen_2_gc_count", gen2Diff, SampleRate);
            _statsd.Add<Statsd.Gauge, double>("dotnet_counters.cpu-usage", value.Cpu, SampleRate);
            _statsd.Send();
        }
    }
}
