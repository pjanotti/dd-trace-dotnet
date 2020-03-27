namespace Datadog.RuntimeMetrics
{
    public struct MetricValue
    {
        public Metric Metric;
        public double Value;
        public string[]? Tags;

        public MetricValue(Metric metric, double value)
            : this(metric, value, tags: null)
        {
            Metric = metric;
            Value = value;
        }

        public MetricValue(Metric metric, double value, string[]? tags)
        {
            Metric = metric;
            Value = value;
            Tags = tags;
        }
    }
}
