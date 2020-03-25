using System;
using System.Runtime.ExceptionServices;
using StatsdClient;

namespace dotnet_metrics
{
    public class CounterDataStatsdShipper : IObserver<CounterData>
    {
        private readonly IStatsd _statsd;

        public CounterDataStatsdShipper(IStatsd statsd)
        {
            _statsd = statsd ?? throw new ArgumentNullException(nameof(statsd));
        }

        public void OnCounterUpdate(CounterData counterData)
        {
            string[] tags =
            {
                $"process-id:{counterData.ProcessId}",
                $"process-name:{counterData.ProcessName}"
            };

            switch (counterData.Type)
            {
                case CounterType.Sum:
                    _statsd.Add<Statsd.Counting, double>(counterData.CounterName, counterData.Value, sampleRate: 1d, tags: tags);
                    break;
                case CounterType.Mean:
                    _statsd.Add<Statsd.Gauge, double>(counterData.CounterName, counterData.Value, sampleRate: 1d, tags: tags);
                    break;
            }

            _statsd.Send();
        }

        /// <summary>Notifies the observer that the provider has finished sending push-based notifications.</summary>
        public void OnCompleted()
        {
        }

        /// <summary>Notifies the observer that the provider has experienced an error condition.</summary>
        /// <param name="error">An object that provides additional information about the error.</param>
        public void OnError(Exception error)
        {
            ExceptionDispatchInfo.Throw(error);
        }

        /// <summary>Provides the observer with new data.</summary>
        /// <param name="value">The current notification information.</param>
        public void OnNext(CounterData value)
        {
            OnCounterUpdate(value);
        }
    }
}
