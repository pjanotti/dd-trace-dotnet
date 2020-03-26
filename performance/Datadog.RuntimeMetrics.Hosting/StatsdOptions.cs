namespace Datadog.RuntimeMetrics.Hosting
{
    public class StatsdOptions
    {
        // DD_AGENT_HOST
        public string Host { get; set; } = "localhost";

        // DD_DOGSTATSD_PORT
        public int Port { get; set; } = 8125;

        public string[] Tags { get; set; } = { };
    }
}
