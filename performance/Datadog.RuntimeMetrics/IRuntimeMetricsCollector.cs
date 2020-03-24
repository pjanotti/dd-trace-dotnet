namespace Datadog.RuntimeMetrics
{
    public interface IRuntimeMetricsCollector
    {
        RuntimeMetrics GetRuntimeMetrics();
    }
}
