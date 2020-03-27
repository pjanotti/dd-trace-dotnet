using System;
using System.Diagnostics.Tracing;

namespace Datadog.RuntimeMetrics
{
    // Microsoft-Windows-DotNETRuntime
    // System.Runtime

    public sealed class SimpleEventListener : EventListener
    {
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            Console.WriteLine($"EventSource added: {eventSource.Name} {eventSource.Guid}");

            if (eventSource.Name.Equals("Microsoft-Windows-DotNETRuntime"))
            {
                //EnableEvents(eventSource, EventLevel.Verbose, (EventKeywords)(-1));
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            // eventData.Payload
        }
    }
}
