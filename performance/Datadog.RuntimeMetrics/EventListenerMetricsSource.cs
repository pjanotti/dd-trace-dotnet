using System;
using System.Collections.Generic;

namespace Datadog.RuntimeMetrics
{
    public class EventListenerMetricsSource : IObservable<IEnumerable<MetricValue>>
    {
        private readonly List<IObserver<IEnumerable<MetricValue>>> _observers = new List<IObserver<IEnumerable<MetricValue>>>();

        public IDisposable Subscribe(IObserver<IEnumerable<MetricValue>> observer)
        {
            _observers.Add(observer);
            return new MetricsUnsubscriber(_observers, observer);
        }

        public void Start()
        {

        }
    }
}
