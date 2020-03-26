using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Datadog.RuntimeMetrics.Hosting
{
    /// <summary>
    /// Wrapper for <see cref="GcMetricsSource"/> that implements <see cref="IHostedService"/>.
    /// </summary>
    public class GcMetricsHostedService : IHostedService
    {
        private readonly GcMetricsSource _service;

        public GcMetricsHostedService(GcMetricsSource service)
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
