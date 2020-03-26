using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using StatsdClient;

namespace Datadog.RuntimeMetrics.Hosting
{
    public class StatsdUdpWrapper : IStatsdUDP, IDisposable
    {
        private const string UnixDomainSocketPrefix = "unix://";

        private readonly IStatsdUDP _statsdUdp;

        public StatsdUdpWrapper(IOptions<StatsdOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.Value.Agent_Host.StartsWith(UnixDomainSocketPrefix))
            {
                var udsType = Type.GetType("StatsdUnixDomainSocket", throwOnError: false);
                ConstructorInfo? constructor = udsType?.GetConstructor(new[] { typeof(string), typeof(int) });
                object? statsUds = constructor?.Invoke(new object[] { options.Value.Agent_Host, 2048 });

                if (statsUds is IStatsdUDP statsd)
                {
                    _statsdUdp = statsd;
                }
                else
                {
                    throw new ArgumentException("Could not create StatsdUnixDomainSocket instance.");
                }
            }
            else
            {
                _statsdUdp = new StatsdUDP(options.Value.Agent_Host, options.Value.Dogstatsd_Port);
            }
        }

        public void Send(string command)
        {
            _statsdUdp.Send(command);
        }

        public Task SendAsync(string command)
        {
            return _statsdUdp.SendAsync(command);
        }

        public void Dispose()
        {
            (_statsdUdp as IDisposable)?.Dispose();
        }
    }
}
