using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using StatsdClient;

namespace Datadog.RuntimeMetrics.Hosting
{
    public class StatsdMetricsSubscriberWrapper : IObserver<IEnumerable<MetricValue>>, IDisposable
    {
        private readonly StatsdMetricsSubscriber _subscriber;

        public StatsdMetricsSubscriberWrapper(IStatsdUDP statsdUdp, IOptions<StatsdOptions> statsdOptions, IOptions<TracingOptions> tracingOptions)
        {
            bool diagnosticSourceEnabled = tracingOptions.Value.DD_DIAGNOSTIC_SOURCE_ENABLED;
            bool manualSpansEnabled = tracingOptions.Value.DD_MANUAL_SPANS_ENABLED;
            string tracerVersion = tracingOptions.Value.DD_TRACER_VERSION;

            var internalTags = new List<string>
                               {
                                   $"service_name:{statsdOptions.Value.ServiceName}"
                               };

            if (diagnosticSourceEnabled)
            {
                internalTags.Add("tracer_mode:diagnostic-source");
                internalTags.Add($"tracer_version:{tracerVersion}");
            }
            else if (manualSpansEnabled)
            {
                internalTags.Add("tracer_mode:manual");
                internalTags.Add($"tracer_version:{tracerVersion}");
            }
            else
            {
                internalTags.Add("tracer_mode:none");
                internalTags.Add("tracer_version:none");
            }

            string[] tags = internalTags.Concat(statsdOptions.Value.Tags).ToArray();
            _subscriber = new StatsdMetricsSubscriber(statsdUdp, statsdOptions.Value.SampleRate, tags);
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
            _subscriber?.Dispose();
        }
    }
}
