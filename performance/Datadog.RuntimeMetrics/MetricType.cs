using System;
using System.Diagnostics;

namespace Datadog.RuntimeMetrics
{
    [DebuggerDisplay("Name = {Name}, Value = {Value}")]
    public class MetricType
    {
        public static readonly MetricType Counting = new MetricType("Counting", "c");
        public static readonly MetricType Timing = new MetricType("Timing", "ms");
        public static readonly MetricType Gauge = new MetricType("Gauge", "g");
        public static readonly MetricType Histogram = new MetricType("Histogram", "h");
        public static readonly MetricType Distribution = new MetricType("Distribution", "d");
        public static readonly MetricType Meter = new MetricType("Meter", "m");
        public static readonly MetricType Set = new MetricType("Set", "s");

        public string Name { get; }
        public string Value { get; }

        public MetricType(string name, string value)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
