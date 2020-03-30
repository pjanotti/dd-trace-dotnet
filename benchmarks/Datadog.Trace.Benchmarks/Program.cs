using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Datadog.Trace.ExtensionMethods;

namespace Datadog.Trace.Benchmarks
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<Tests>();
        }
    }

    [MemoryDiagnoser]
    [GcServer(true)]
    [GcForce(false)]
    public class Tests
    {
        [Params(1, 5, 10, 20, 40)]
        public int SpanCount { get; set; }

        private MethodInfo Flush = typeof(Tracer).GetMethod("FlushAsync", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        [Benchmark]
        public void WithTraces()
        {
            using (Scope rootScope = Tracer.Instance.StartActive("root"))
            {
                Span rootSpan = rootScope.Span;
                rootSpan.Type = SpanTypes.Custom;
                rootSpan.SetTag("traceIndex", "0");
                rootSpan.SetTag("key1", "value1");
                rootSpan.SetTag("key2", "value2");
                rootSpan.SetTraceSamplingPriority(SamplingPriority.UserKeep);

                for (int spanIndex = 0; spanIndex < SpanCount - 1; spanIndex++)
                {
                    using (Scope childScope = Tracer.Instance.StartActive("child"))
                    {
                        Span childSpan = childScope.Span;
                        childSpan.Type = SpanTypes.Custom;
                        childSpan.SetTag("spanIndex", spanIndex.ToString());
                        childSpan.SetTag("key1", "value1");
                        childSpan.SetTag("key2", "value2");

                        Thread.Sleep(5);
                    }

                    Thread.Sleep(5);
                }

                Thread.Sleep(5);
            }

            var task = Flush.Invoke(Tracer.Instance, null) as Task;
            task.GetAwaiter().GetResult();
        }
    }
}
