using System.Threading;
using System.Threading.Tasks;

namespace Datadog.RuntimeMetrics
{
    public interface IBackgroundService
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}
