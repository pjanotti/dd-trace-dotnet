using System;

namespace Datadog.RuntimeMetrics
{
    public class MetricType
    {
        public static readonly MetricType Counting = new MetricType("c");
        public static readonly MetricType Timing = new MetricType("ms");
        public static readonly MetricType Gauge = new MetricType("g");
        public static readonly MetricType Histogram = new MetricType("h");
        public static readonly MetricType Distribution = new MetricType("d");
        public static readonly MetricType Meter = new MetricType("m");
        public static readonly MetricType Set = new MetricType("s");

        public string Value { get; }

        public MetricType(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
