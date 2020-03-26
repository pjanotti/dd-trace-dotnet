namespace Datadog.RuntimeMetrics
{
    public interface IRuntimeMetricsCollector
    {
        GcMetrics GetRuntimeMetrics();
    }
}
