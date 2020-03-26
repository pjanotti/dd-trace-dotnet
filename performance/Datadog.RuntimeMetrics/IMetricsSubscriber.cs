using System;
using System.Collections.Generic;

namespace Datadog.RuntimeMetrics
{
    public interface IMetricsSubscriber : IObserver<IEnumerable<MetricValue>>
    {
    }
}
