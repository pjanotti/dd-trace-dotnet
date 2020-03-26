using System;
using System.Collections.Generic;
using Datadog.Trace;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using StatsdClient;

namespace Datadog.RuntimeMetrics.Hosting
{
    public class StatsdMetricsSubscriberWrapper : IMetricsSubscriber, IDisposable
    {
        private readonly StatsdMetricsSubscriber _subscriber;
        private readonly IDisposable? _subscription;

        public StatsdMetricsSubscriberWrapper(IStatsdUDP statsdUdp, Tracer tracer, IMetricsSource? source, IOptions<StatsdOptions> options, IConfiguration configuration)
        {
            bool diagnosticSourceEnabled = configuration.GetValue("DD_DIAGNOSTIC_SOURCE_ENABLED", false);
            bool middlewareEnabled = configuration.GetValue("DD_MIDDLEWARE_ENABLED", false);
            string tracerVersion = configuration.GetValue("DD_TRACER_VERSION", "latest");

            var internalTags = new List<string>
                               {
                                   $"service_name:{tracer.DefaultServiceName}"
                               };

            if (diagnosticSourceEnabled)
            {
                internalTags.Add("tracer_mode:diagnostic-source");
                internalTags.Add($"tracer_version:{tracerVersion}");
            }
            else if (middlewareEnabled)
            {
                internalTags.Add("tracer_mode:middleware");
                internalTags.Add($"tracer_version:{tracerVersion}");
            }
            else
            {
                internalTags.Add("tracer_mode:none");
                internalTags.Add("tracer_version:none");
            }

            _subscriber = new StatsdMetricsSubscriber(statsdUdp, internalTags.ToArray(), sampleRate: 1d);
            _subscription = source?.Subscribe(_subscriber);
        }

        public void OnCompleted()
        {
            _subscriber.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _subscriber.OnError(error);
        }

        public void OnNext(IEnumerable<MetricValue> value)
        {
            _subscriber.OnNext(value);
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscriber?.Dispose();
        }
    }
}
