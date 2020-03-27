// ReSharper disable InconsistentNaming

namespace Datadog.RuntimeMetrics
{
    public class TracingOptions
    {
        public bool DD_DIAGNOSTIC_SOURCE_ENABLED { get; set; }

        public bool DD_MIDDLEWARE_ENABLED { get; set; }

        public bool DD_MANUAL_SPANS_ENABLED { get; set; }
    }
}
