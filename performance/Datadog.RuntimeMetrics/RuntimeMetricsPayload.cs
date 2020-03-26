namespace Datadog.RuntimeMetrics
{
    public struct RuntimeMetricValue
    {
        public RuntimeMetric Metric;
        public double Value;

        public RuntimeMetricValue(RuntimeMetric metric, double value)
        {
            Metric = metric;
            Value = value;
        }
    }
}
