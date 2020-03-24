using Microsoft.Extensions.Hosting;

namespace Datadog.RuntimeMetrics.Hosting
{
    public class RuntimeMetricsHostedService : RuntimeMetricsService, IHostedService
    {
        public RuntimeMetricsHostedService(IRuntimeMetricsCollector collector) : base(collector)
        {
        }
    }
}
