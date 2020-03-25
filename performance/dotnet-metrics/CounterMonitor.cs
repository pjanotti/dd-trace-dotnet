using System;
using System.Collections.Generic;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;

namespace dotnet_metrics
{
    public sealed class CounterMonitor : IObservable<CounterData>, IDisposable
    {
        private readonly List<IObserver<CounterData>> _observers = new List<IObserver<CounterData>>();
        private readonly int _processId;
        private readonly string _processName;
        private EventPipeSession _session;

        public CounterMonitor(int processId, string processName)
        {
            _processId = processId;
            _processName = processName ?? throw new ArgumentNullException(nameof(processName));
        }

        public void Start()
        {
            if (_session != null)
            {
                throw new InvalidOperationException("Session already started.");
            }

            EventPipeProvider[] providers =
            {
                CounterHelpers.MakeProvider("System.Runtime", 1)
            };

            var client = new DiagnosticsClient(_processId);
            _session = client.StartEventPipeSession(providers);
            var source = new EventPipeEventSource(_session.EventStream);
            source.Dynamic.All += ProcessEvents;

            // this is a blocking call
            source.Process();
        }

        public void Stop()
        {
            if (_session == null)
            {
                throw new InvalidOperationException("Start() must be called to start the session.");
            }

            _session.Stop();
            _session.Dispose();
            _session = null;
        }

        public void Dispose()
        {
            _session?.Dispose();
        }

        private void ProcessEvents(TraceEvent data)
        {
            if (data.EventName.Equals("EventCounters"))
            {
                var payload = (IDictionary<string, object>)data.PayloadValue(0);
                var payloadValues = (IDictionary<string, object>)payload["Payload"];
                // the TraceEvent implementation throws not implemented exception if you try
                // to get the list of the dictionary keys: it is needed to iterate on the dictionary
                // and get each key/value pair.

                var name = payloadValues["Name"] as string;
                var displayName = payloadValues["DisplayName"] as string;
                var counterType = payloadValues["CounterType"] as string;

                if (counterType == "Sum")
                {
                    double value = GetDouble(payloadValues["Increment"]);
                    OnNext(name, displayName, value);
                }
                else if (counterType == "Mean")
                {
                    double value = GetDouble(payloadValues["Mean"]);
                    OnNext(name, displayName, value);
                }
                else
                {
                    var exception = new InvalidOperationException($"Unsupported counter type '{counterType}'");

                    foreach (IObserver<CounterData> observer in _observers)
                    {
                        observer.OnError(exception);
                    }
                }
            }
        }

        private double GetDouble(object value)
        {
            if (value is double d)
            {
                return d;
            }

            return double.TryParse(value.ToString(), out d) ? d : double.NaN;
        }

        private void OnNext(string counterName, string counterDisplayName, double value)
        {
            var args = new CounterData(_processId, _processName, counterName, counterDisplayName, CounterType.Sum, value);

            foreach (IObserver<CounterData> observer in _observers)
            {
                observer.OnNext(args);
            }
        }

        /// <summary>Notifies the provider that an observer is to receive notifications.</summary>
        /// <param name="observer">The object that is to receive notifications.</param>
        /// <returns>A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.</returns>
        public IDisposable Subscribe(IObserver<CounterData> observer)
        {
            _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }

        private class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<CounterData>> _observers;
            private readonly IObserver<CounterData> _observer;

            public Unsubscriber(List<IObserver<CounterData>> observers, IObserver<CounterData> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose()
            {
                _observers?.Remove(_observer);
            }
        }
    }
}
