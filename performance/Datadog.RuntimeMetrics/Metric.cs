using System;
using System.Diagnostics;

namespace Datadog.RuntimeMetrics
{
    [DebuggerDisplay("Name = {Name}, Type = {Type.Name}")]
    public class Metric
    {
        public static readonly Metric GcHeapSize = new Metric("dotnet_counters.gc_heap_size", MetricType.Gauge);
        public static readonly Metric GcCountGen0 = new Metric("dotnet_counters.gen_0_gc_count", MetricType.Counting);
        public static readonly Metric GcCountGen1 = new Metric("dotnet_counters.gen_1_gc_count", MetricType.Counting);
        public static readonly Metric GcCountGen2 = new Metric("dotnet_counters.gen_2_gc_count", MetricType.Counting);
        public static readonly Metric GcSizeGen0 = new Metric("dotnet_counters.gc_size.gen_0", MetricType.Gauge);
        public static readonly Metric GcSizeGen1 = new Metric("dotnet_counters.gc_size.gen_1", MetricType.Gauge);
        public static readonly Metric GcSizeGen2 = new Metric("dotnet_counters.gc_size.gen_2", MetricType.Gauge);
        public static readonly Metric GcSizeLoh = new Metric("dotnet_counters.gc_size.loh", MetricType.Gauge);
        public static readonly Metric WorkingSet = new Metric("dotnet_counters.working_set", MetricType.Gauge);
        public static readonly Metric PrivateBytes = new Metric("dotnet_counters.private_bytes", MetricType.Gauge);
        public static readonly Metric CpuPercent = new Metric("dotnet_counters.cpu_percent", MetricType.Gauge);
        public static readonly Metric CpuTimeMs = new Metric("dotnet_counters.cpu_time", MetricType.Counting);
        public static readonly Metric AllocatedBytes = new Metric("dotnet_counters.alloc", MetricType.Counting);

        public string Name { get; }
        public MetricType Type { get; }

        public Metric(string name, MetricType type)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
