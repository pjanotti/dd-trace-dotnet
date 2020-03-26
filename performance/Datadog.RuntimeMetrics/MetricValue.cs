namespace Datadog.RuntimeMetrics
{
    public struct MetricValue
    {
        public Metric Metric;
        public double Value;

        public MetricValue(Metric metric, double value)
        {
            Metric = metric;
            Value = value;
        }
    }
}
