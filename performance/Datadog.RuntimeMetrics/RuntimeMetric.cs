using System;

namespace Datadog.RuntimeMetrics
{
    public class RuntimeMetric
    {
        public static readonly RuntimeMetric GcHeapSize = new RuntimeMetric("dotnet_counters.gc_heap_size", RuntimeMetricType.Gauge);
        public static readonly RuntimeMetric WorkingSet = new RuntimeMetric("dotnet_counters.working_set", RuntimeMetricType.Gauge);
        public static readonly RuntimeMetric PrivateBytes = new RuntimeMetric("dotnet_counters.private_bytes", RuntimeMetricType.Gauge);
        public static readonly RuntimeMetric GcCountGen0 = new RuntimeMetric("dotnet_counters.gen_0_gc_count", RuntimeMetricType.Counting);
        public static readonly RuntimeMetric GcCountGen1 = new RuntimeMetric("dotnet_counters.gen_1_gc_count", RuntimeMetricType.Counting);
        public static readonly RuntimeMetric GcCountGen2 = new RuntimeMetric("dotnet_counters.gen_2_gc_count", RuntimeMetricType.Counting);
        public static readonly RuntimeMetric CpuUsage = new RuntimeMetric("dotnet_counters.cpu_usage", RuntimeMetricType.Gauge);

        public string Name { get; }
        public RuntimeMetricType Type { get; }

        public RuntimeMetric(string name, RuntimeMetricType type)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }
    }
}
