using System;
using System.Collections.Generic;

namespace Datadog.RuntimeMetrics
{
    public interface IMetricsSource : IObservable<IEnumerable<MetricValue>>
    {
    }
}
