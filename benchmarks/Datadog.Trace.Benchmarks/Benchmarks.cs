using System.Reflection;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Datadog.Trace.ExtensionMethods;

namespace Datadog.Trace.Benchmarks
{
    [MemoryDiagnoser]
    [GcServer(true)]
    [GcForce(true)]
    //[SimpleJob(RuntimeMoniker.NetCoreApp21)]
    //[SimpleJob(RuntimeMoniker.NetCoreApp31)]
    //[SimpleJob(RuntimeMoniker.Net48)]
    //[Config(typeof(Config))]
    public class Benchmarks
    {
        [Params(100, 200)]
        public int TraceCount { get; set; }

        [Params(1, 10)]
        public int SpansPerTrace { get; set; }

        private static readonly MethodInfo Flush = typeof(Tracer).GetMethod("FlushAsync", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        [Benchmark]
        public void WithTraces()
        {
            for (int traceIndex = 0; traceIndex < TraceCount; traceIndex++)
            {
                using (Scope rootScope = Tracer.Instance.StartActive("root"))
                {
                    Span rootSpan = rootScope.Span;
                    rootSpan.Type = SpanTypes.Custom;
                    rootSpan.SetTag("traceIndex", "0");
                    AddTags(rootSpan, 10);
                    rootSpan.SetTraceSamplingPriority(SamplingPriority.UserReject);

                    for (int spanIndex = 0; spanIndex < SpansPerTrace - 1; spanIndex++)
                    {
                        using (Scope childScope = Tracer.Instance.StartActive("child"))
                        {
                            Span childSpan = childScope.Span;
                            childSpan.Type = SpanTypes.Custom;
                            childSpan.SetTag("spanIndex", spanIndex.ToString());
                            AddTags(rootSpan, 10);
                        }
                    }
                }
            }

            ((Task)Flush.Invoke(Tracer.Instance, null)).GetAwaiter().GetResult();
        }

        private void AddTags(Span span, int count)
        {
            for (int i = 0; i < count; i++)
            {
                span.SetTag($"key{i}", $"value{i}");
            }
        }
    }
}
