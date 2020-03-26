using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Datadog.RuntimeMetrics
{
    public class RuntimeMetricsGcService : BackgroundService, IObservable<IEnumerable<RuntimeMetricValue>>
    {
        private readonly List<IObserver<IEnumerable<RuntimeMetricValue>>> _observers = new List<IObserver<IEnumerable<RuntimeMetricValue>>>();
        private readonly TimeSpan _period = TimeSpan.FromSeconds(1);
        private readonly IRuntimeMetricsCollector _collector;

        public RuntimeMetricsGcService(IRuntimeMetricsCollector collector)
        {
            _collector = collector ?? throw new ArgumentNullException(nameof(collector));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Factory.StartNew(() =>
                                         {
                                             var values = new RuntimeMetricValue[7];

                                             while (!stoppingToken.IsCancellationRequested)
                                             {
                                                 GcMetrics metrics = _collector.GetRuntimeMetrics();

                                                 values[0] = new RuntimeMetricValue(RuntimeMetric.GcHeapSize, metrics.Allocated);
                                                 values[1] = new RuntimeMetricValue(RuntimeMetric.WorkingSet, metrics.WorkingSet);
                                                 values[2] = new RuntimeMetricValue(RuntimeMetric.PrivateBytes, metrics.PrivateBytes);
                                                 values[3] = new RuntimeMetricValue(RuntimeMetric.GcCountGen0, metrics.GcCountGen0);
                                                 values[4] = new RuntimeMetricValue(RuntimeMetric.GcCountGen1, metrics.GcCountGen1);
                                                 values[5] = new RuntimeMetricValue(RuntimeMetric.GcCountGen2, metrics.GcCountGen2);
                                                 values[6] = new RuntimeMetricValue(RuntimeMetric.CpuUsage, metrics.CpuUsage);

                                                 foreach (IObserver<IEnumerable<RuntimeMetricValue>> observer in _observers)
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


        public IDisposable Subscribe(IObserver<IEnumerable<RuntimeMetricValue>> observer)
        {
            _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }

        private class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<IEnumerable<RuntimeMetricValue>>> _observers;
            private readonly IObserver<IEnumerable<RuntimeMetricValue>> _observer;

            public Unsubscriber(List<IObserver<IEnumerable<RuntimeMetricValue>>> observers, IObserver<IEnumerable<RuntimeMetricValue>> observer)
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
