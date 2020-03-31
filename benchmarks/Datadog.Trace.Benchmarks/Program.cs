using System;
using BenchmarkDotNet.Running;

namespace Datadog.Trace.Benchmarks
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchmarks>();
        }
    }
}
