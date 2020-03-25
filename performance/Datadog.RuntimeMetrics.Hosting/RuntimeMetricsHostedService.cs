using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Datadog.RuntimeMetrics.Hosting
{
    /// <summary>
    /// Wrapper for <see cref="RuntimeMetricsService"/> that implements <see cref="IHostedService"/>.
    /// </summary>
    public class RuntimeMetricsHostedService :  IHostedService
    {
        private readonly RuntimeMetricsService _service;

        public RuntimeMetricsHostedService(RuntimeMetricsService service)
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
