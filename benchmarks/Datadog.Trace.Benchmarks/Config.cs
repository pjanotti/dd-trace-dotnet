using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace Datadog.Trace.Benchmarks
{
    public class Config : ManualConfig
    {
        public Config()
        {
            Add(Job.Default.WithNuGet("Datadog.Trace", "1.15.0").WithId("1.15.0"));
            Add(Job.Default.WithNuGet("Datadog.Trace", "1.15.1-prerelease").WithId("1.15.1-prerelease"));
        }
    }
}
