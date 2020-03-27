using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

// EventSource:
// Microsoft-Windows-DotNETRuntime 5e5bb766-bbfc-5662-0548-1d44fad9bb56
// System.Runtime                  49592c0f-5a05-516d-aa4b-a64e02026c89

// https://stebet.net/monitoring-gc-and-memory-allocations-with-net-core-2-2-and-application-insights/
// https://medium.com/criteo-labs/c-in-process-clr-event-listeners-with-net-core-2-2-ef4075c14e87
// https://github.com/dotnet/coreclr/blob/master/src/vm/ClrEtwAll.man

namespace Datadog.RuntimeMetrics
{
    public sealed class GcEventListener : EventListener, IObservable<IEnumerable<MetricValue>>
    {
        private const int GC_KEYWORD = 0x0000001;

        private readonly ObserverCollection<IEnumerable<MetricValue>> _observers = new ObserverCollection<IEnumerable<MetricValue>>();

        private bool _eventsEnabled;
        private EventSource? _eventSource;

        public void EnableEvents()
        {
            _eventsEnabled = true;

            if (_eventSource != null)
            {
                EnableEvents(_eventSource, EventLevel.Verbose, (EventKeywords)GC_KEYWORD);
            }
        }

        public void DisableEvents()
        {
            _eventsEnabled = true;

            if (_eventSource != null)
            {
                DisableEvents(_eventSource);
            }
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name == "Microsoft-Windows-DotNETRuntime")
            {
                _eventSource = eventSource;

                if (_eventsEnabled)
                {
                    EnableEvents(eventSource, EventLevel.Verbose, (EventKeywords)GC_KEYWORD);
                }
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (!_eventsEnabled)
            {
                return;
            }

            switch (eventData.EventName)
            {
                case "GCHeapStats_V1":
                    ProcessHeapStats(eventData);
                    break;
                case "GCAllocationTick_V3":
                    ProcessAllocationEvent(eventData);
                    break;
            }
        }

        private void ProcessHeapStats(EventWrittenEventArgs eventData)
        {
            var metrics = new[]
                          {
                              new MetricValue(Metric.GcSizeGen0, (ulong)eventData.Payload[0]),
                              new MetricValue(Metric.GcSizeGen1, (ulong)eventData.Payload[2]),
                              new MetricValue(Metric.GcSizeGen2, (ulong)eventData.Payload[4]),
                              new MetricValue(Metric.GcSizeLoh, (ulong)eventData.Payload[6])
                          };

            _observers.OnNext(metrics);
        }

        private void ProcessAllocationEvent(EventWrittenEventArgs eventData)
        {
            var tags = new[]
                       {
                           $"type-name:{(string)eventData.Payload[5]}"
                       };

            var metrics = new[]
                          {
                              new MetricValue(Metric.AllocatedBytes, (ulong)eventData.Payload[3], tags)
                          };

            _observers.OnNext(metrics);
        }

        public IDisposable Subscribe(IObserver<IEnumerable<MetricValue>> observer)
        {
            return _observers.Subscribe(observer);
        }
    }
}
