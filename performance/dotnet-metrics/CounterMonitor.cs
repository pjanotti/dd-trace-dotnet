using System;
using System.Collections.Generic;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;

namespace dotnet_metrics
{
    public sealed class CounterMonitor : IDisposable
    {
        private readonly int _processId;
        private readonly string _processName;
        private readonly ICounterSink _counterSink;
        private EventPipeSession _session;

        public CounterMonitor(int processId, string processName, ICounterSink counterSink)
        {
            _processId = processId;
            _processName = processName ?? throw new ArgumentNullException(nameof(processName));
            _counterSink = counterSink ?? throw new ArgumentNullException(nameof(counterSink));
        }

        public void Start()
        {
            if (_session != null)
            {
                throw new InvalidOperationException("Session already started.");
            }

            EventPipeProvider[] providers = { CounterHelpers.MakeProvider("System.Runtime", 1) };

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
                var countersPayload = (IDictionary<string, object>)(data.PayloadValue(0));
                var kvPairs = (IDictionary<string, object>)(countersPayload["Payload"]);
                // the TraceEvent implementation throws not implemented exception if you try
                // to get the list of the dictionary keys: it is needed to iterate on the dictionary
                // and get each key/value pair.

                var name = string.Intern(kvPairs["Name"].ToString());
                var displayName = string.Intern(kvPairs["DisplayName"].ToString());
                var counterType = kvPairs["CounterType"];

                if (counterType.Equals("Sum"))
                {
                    OnSumCounter(name, displayName, kvPairs);
                }
                else if (counterType.Equals("Mean"))
                {
                    OnMeanCounter(name, displayName, kvPairs);
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported counter type '{counterType}'");
                }
            }
        }

        private void OnSumCounter(string counterName, string counterDisplayName, IDictionary<string, object> kvPairs)
        {
            double value = double.Parse(kvPairs["Increment"].ToString());

            // send the information to your metrics pipeline
            _counterSink.OnCounterUpdate(new CounterEventArgs(_processId, _processName, counterName, counterDisplayName, CounterType.Sum, value));
        }

        private void OnMeanCounter(string name, string displayName, IDictionary<string, object> kvPairs)
        {
            double value = double.Parse(kvPairs["Mean"].ToString());

            // send the information to your metrics pipeline
            _counterSink.OnCounterUpdate(new CounterEventArgs(_processId, _processName, name, displayName, CounterType.Mean, value));
        }
    }
}
