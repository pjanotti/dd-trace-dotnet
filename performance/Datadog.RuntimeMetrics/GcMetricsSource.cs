using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Datadog.RuntimeMetrics
{
    public class GcMetricsSource : BackgroundService, IMetricsSource
    {
        private readonly List<IObserver<IEnumerable<MetricValue>>> _observers = new List<IObserver<IEnumerable<MetricValue>>>();
        private readonly TimeSpan _period = TimeSpan.FromSeconds(1);
        private readonly IMetricsProvider<GcMetrics> _provider;

        public GcMetricsSource(IMetricsProvider<GcMetrics> provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Factory.StartNew(() =>
                                         {
                                             var values = new MetricValue[7];

                                             while (!stoppingToken.IsCancellationRequested)
                                             {
                                                 GcMetrics metrics = _provider.GetMetrics();

                                                 values[0] = new MetricValue(Metric.GcHeapSize, metrics.Allocated);
                                                 values[1] = new MetricValue(Metric.WorkingSet, metrics.WorkingSet);
                                                 values[2] = new MetricValue(Metric.PrivateBytes, metrics.PrivateBytes);
                                                 values[3] = new MetricValue(Metric.GcCountGen0, metrics.GcCountGen0);
                                                 values[4] = new MetricValue(Metric.GcCountGen1, metrics.GcCountGen1);
                                                 values[5] = new MetricValue(Metric.GcCountGen2, metrics.GcCountGen2);
                                                 values[6] = new MetricValue(Metric.CpuUsage, metrics.CpuUsage);

                                                 foreach (IObserver<IEnumerable<MetricValue>> observer in _observers)
                                                 {
                                                     observer.OnNext(values);
                                                 }

                                                 if (!stoppingToken.IsCancellationRequested)
                                                 {
                                                     Thread.Sleep(_period);
                                                 }
                                             }
                                         },
                                         stoppingToken,
                                         TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach,
                                         TaskScheduler.Default);
        }


        public IDisposable Subscribe(IObserver<IEnumerable<MetricValue>> observer)
        {
            _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }

        private class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<IEnumerable<MetricValue>>> _observers;
            private readonly IObserver<IEnumerable<MetricValue>> _observer;

            public Unsubscriber(List<IObserver<IEnumerable<MetricValue>>> observers, IObserver<IEnumerable<MetricValue>> observer)
            {
                _observers = observers ?? throw new ArgumentNullException(nameof(observers));
                _observer = observer ?? throw new ArgumentNullException(nameof(observer));
            }

            /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
            public void Dispose()
            {
                _observers.Remove(_observer);
            }
        }
    }
}
