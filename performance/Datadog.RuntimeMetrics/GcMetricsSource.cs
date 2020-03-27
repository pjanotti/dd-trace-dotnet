using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Datadog.RuntimeMetrics
{
    public class GcMetricsSource : BackgroundService, IObservable<IEnumerable<MetricValue>>
    {
        private readonly List<IObserver<IEnumerable<MetricValue>>> _observers = new List<IObserver<IEnumerable<MetricValue>>>();
        private readonly Process _process = Process.GetCurrentProcess();

        private TimeSpan _oldCpuTime = TimeSpan.Zero;
        private DateTime _lastMonitorTime = DateTime.UtcNow;
        private double _cpu;

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Factory.StartNew(() =>
                                         {
                                             var values = new MetricValue[7];
                                             var period = TimeSpan.FromSeconds(1);

                                             while (!stoppingToken.IsCancellationRequested)
                                             {
                                                 GcMetrics metrics = GetMetrics();

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
                                                     Thread.Sleep(period);
                                                 }
                                             }
                                         },
                                         stoppingToken,
                                         TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach,
                                         TaskScheduler.Default);
        }

        public GcMetrics GetMetrics()
        {
            DateTime now = DateTime.UtcNow;
            _process.Refresh();

            double cpuElapsedTime = now.Subtract(_lastMonitorTime).TotalMilliseconds;

            TimeSpan newCpuTime = _process.TotalProcessorTime;
            double elapsedCpu = (newCpuTime - _oldCpuTime).TotalMilliseconds;
            _cpu = elapsedCpu * 100 / Environment.ProcessorCount / cpuElapsedTime;

            _lastMonitorTime = now;
            _oldCpuTime = newCpuTime;

            var metrics = new GcMetrics
                          {
                              Allocated = GC.GetTotalMemory(false),
                              WorkingSet = _process.WorkingSet64,
                              PrivateBytes = _process.PrivateMemorySize64,
                              GcCountGen0 = GC.CollectionCount(0),
                              GcCountGen1 = GC.CollectionCount(1),
                              GcCountGen2 = GC.CollectionCount(2),
                              CpuUsage = _cpu
                          };

            return metrics;
        }

        public IDisposable Subscribe(IObserver<IEnumerable<MetricValue>> observer)
        {
            _observers.Add(observer);
            return new MetricsUnsubscriber(_observers, observer);
        }

        protected override void Dispose(bool disposing)
        {
            _process?.Dispose();
            base.Dispose(disposing);
        }
    }
}
