// ReSharper disable InconsistentNaming

namespace Datadog.RuntimeMetrics
{
    public class TracingOptions
    {
        // DD_DIAGNOSTIC_SOURCE_ENABLED
        public bool Diagnostic_Source_Enabled { get; set; }

        // DD_MIDDLEWARE_ENABLED
        public bool Middleware_Enabled { get; set; }

        // DD_MANUAL_SPANS_ENABLED
        public bool Manual_Spans_Enabled { get; set; }
    }
}
