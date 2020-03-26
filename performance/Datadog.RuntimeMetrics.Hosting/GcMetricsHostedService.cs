using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Datadog.RuntimeMetrics.Hosting
{
    /// <summary>
    /// Wrapper for <see cref="GcMetricsSource"/> that implements <see cref="IBackgroundService"/>.
    /// </summary>
    public class GcMetricsHostedService : IHostedService
    {
        private readonly IMetricsSourceBackgroundService _service;

        public GcMetricsHostedService(IMetricsSourceBackgroundService service)
        {
            _service = service;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _service.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _service.StopAsync(cancellationToken);
        }
    }
}
