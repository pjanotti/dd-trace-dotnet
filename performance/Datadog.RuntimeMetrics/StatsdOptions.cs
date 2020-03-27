// ReSharper disable InconsistentNaming

using System.Collections.Generic;
using System.Linq;

namespace Datadog.RuntimeMetrics
{
    public class StatsdOptions
    {
        public string DD_AGENT_HOST { get; set; } = "localhost";

        public int DD_DOGSTATSD_PORT { get; set; } = 8125;

        public string? ServiceName { get; set; }

        public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();

        public double? SampleRate { get; set; } = 1d;
    }
}
