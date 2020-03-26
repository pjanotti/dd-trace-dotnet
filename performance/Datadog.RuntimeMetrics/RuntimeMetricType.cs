using System;

namespace Datadog.RuntimeMetrics
{
    public class RuntimeMetricType
    {
        public static readonly RuntimeMetricType Counting = new RuntimeMetricType("c");
        public static readonly RuntimeMetricType Timing = new RuntimeMetricType("ms");
        public static readonly RuntimeMetricType Gauge = new RuntimeMetricType("g");
        public static readonly RuntimeMetricType Histogram = new RuntimeMetricType("h");
        public static readonly RuntimeMetricType Distribution = new RuntimeMetricType("d");
        public static readonly RuntimeMetricType Meter = new RuntimeMetricType("m");
        public static readonly RuntimeMetricType Set = new RuntimeMetricType("s");

        public string Value { get; }

        public RuntimeMetricType(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
