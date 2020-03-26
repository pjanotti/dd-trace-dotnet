using System;

namespace Datadog.RuntimeMetrics
{
    public class Metric
    {
        public static readonly Metric GcHeapSize = new Metric("dotnet_counters.gc_heap_size", MetricType.Gauge);
        public static readonly Metric WorkingSet = new Metric("dotnet_counters.working_set", MetricType.Gauge);
        public static readonly Metric PrivateBytes = new Metric("dotnet_counters.private_bytes", MetricType.Gauge);
        public static readonly Metric GcCountGen0 = new Metric("dotnet_counters.gen_0_gc_count", MetricType.Counting);
        public static readonly Metric GcCountGen1 = new Metric("dotnet_counters.gen_1_gc_count", MetricType.Counting);
        public static readonly Metric GcCountGen2 = new Metric("dotnet_counters.gen_2_gc_count", MetricType.Counting);
        public static readonly Metric CpuUsage = new Metric("dotnet_counters.cpu_usage", MetricType.Gauge);

        public string Name { get; }
        public MetricType Type { get; }

        public Metric(string name, MetricType type)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }
    }
}
