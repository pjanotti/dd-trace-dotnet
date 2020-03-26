using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Datadog.RuntimeMetrics.Hosting
{
    /// <summary>
    /// Wrapper for <see cref="RuntimeMetricsGcService"/> that implements <see cref="IHostedService"/>.
    /// </summary>
    public class RuntimeMetricsGcHostedService :  IHostedService
    {
        private readonly RuntimeMetricsGcService _service;

        public RuntimeMetricsGcHostedService(RuntimeMetricsGcService service)
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
