// ReSharper disable InconsistentNaming

namespace Datadog.RuntimeMetrics
{
    public class StatsdOptions
    {
        // DD_AGENT_HOST
        public string Agent_Host { get; set; } = "localhost";

        // DD_DOGSTATSD_PORT
        public int Dogstatsd_Port { get; set; } = 8125;
    }
}
