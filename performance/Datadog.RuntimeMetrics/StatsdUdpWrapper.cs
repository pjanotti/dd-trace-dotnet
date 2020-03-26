using System;
using System.Reflection;
using System.Threading.Tasks;
using StatsdClient;

namespace Datadog.RuntimeMetrics
{
    public class StatsdUdpWrapper : IStatsdUDP, IDisposable
    {
        private const string UnixDomainSocketPrefix = "unix://";

        private readonly IStatsdUDP _statsdUdp;
        private readonly StatsdOptions _options;

        public StatsdUdpWrapper(StatsdOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            if (options.Agent_Host.StartsWith("unix://"))
            {
                var udsType = Type.GetType("StatsdUnixDomainSocket", throwOnError: false);
                ConstructorInfo? constructor = udsType?.GetConstructor(new[] { typeof(string), typeof(int) });
                object? statsUds = constructor?.Invoke(new object[] { options.Agent_Host, 2048 });

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
                _statsdUdp = new StatsdUDP(options.Agent_Host, options.Dogstatsd_Port);
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
