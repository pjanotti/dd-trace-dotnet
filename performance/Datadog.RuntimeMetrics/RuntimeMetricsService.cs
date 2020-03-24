using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Datadog.RuntimeMetrics
{
    public class RuntimeMetricsService : BackgroundService, IObservable<RuntimeMetrics>
    {
        private readonly List<IObserver<RuntimeMetrics>> _observers = new List<IObserver<RuntimeMetrics>>();
        private readonly TimeSpan _period = TimeSpan.FromSeconds(1);
        private readonly IRuntimeMetricsCollector _collector;

        public RuntimeMetricsService(IRuntimeMetricsCollector collector)
        {
            _collector = collector ?? throw new ArgumentNullException(nameof(collector));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Factory.StartNew(() =>
                                         {
                                             while (!stoppingToken.IsCancellationRequested)
                                             {
                                                 RuntimeMetrics metrics = _collector.GetRuntimeMetrics();

                                                 foreach (IObserver<RuntimeMetrics> observer in _observers)
                                                 {
                                                     observer.OnNext(metrics);
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


        public IDisposable Subscribe(IObserver<RuntimeMetrics> observer)
        {
            _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }

        private class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<RuntimeMetrics>> _observers;
            private readonly IObserver<RuntimeMetrics> _observer;

            public Unsubscriber(List<IObserver<RuntimeMetrics>> observers, IObserver<RuntimeMetrics> observer)
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
