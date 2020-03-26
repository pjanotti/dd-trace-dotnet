namespace Datadog.RuntimeMetrics
{
    public interface IMetricsProvider<out T>
    {
        T GetMetrics();
    }
}
