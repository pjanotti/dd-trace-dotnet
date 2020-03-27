using System;
using System.Collections.Generic;

namespace Datadog.RuntimeMetrics
{
    internal class MetricsUnsubscriber : IDisposable
    {
        private readonly ICollection<IObserver<IEnumerable<MetricValue>>> _observers;
        private readonly IObserver<IEnumerable<MetricValue>> _observer;

        public MetricsUnsubscriber(ICollection<IObserver<IEnumerable<MetricValue>>> observers, IObserver<IEnumerable<MetricValue>> observer)
        {
            _observers = observers ?? throw new ArgumentNullException(nameof(observers));
            _observer = observer ?? throw new ArgumentNullException(nameof(observer));
        }

        public void Dispose()
        {
            _observers.Remove(_observer);
        }
    }
}
