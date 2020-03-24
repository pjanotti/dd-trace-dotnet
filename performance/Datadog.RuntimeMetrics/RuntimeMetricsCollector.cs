using System;
using System.Diagnostics;

namespace Datadog.RuntimeMetrics
{
    // https://github.com/sebastienros/memoryleak/blob/master/src/MemoryLeak/MemoryLeak/Controllers/DiagnosticsController.cs

    public class RuntimeMetricsCollector : IRuntimeMetricsCollector, IDisposable
    {
        private readonly Process _process = Process.GetCurrentProcess();

        private TimeSpan _oldCpuTime = TimeSpan.Zero;
        private DateTime _lastMonitorTime = DateTime.UtcNow;
        private double _cpu;

        public RuntimeMetrics GetRuntimeMetrics()
        {
            DateTime now = DateTime.UtcNow;
            _process.Refresh();

            double cpuElapsedTime = now.Subtract(_lastMonitorTime).TotalMilliseconds;

            TimeSpan newCpuTime = _process.TotalProcessorTime;
            double elapsedCpu = (newCpuTime - _oldCpuTime).TotalMilliseconds;
            _cpu = elapsedCpu * 100 / Environment.ProcessorCount / cpuElapsedTime;

            _lastMonitorTime = now;
            _oldCpuTime = newCpuTime;

            var metrics = new RuntimeMetrics
                          {
                              Allocated = GC.GetTotalMemory(false),
                              WorkingSet = _process.WorkingSet64,
                              PrivateBytes = _process.PrivateMemorySize64,
                              Gen0 = GC.CollectionCount(0),
                              Gen1 = GC.CollectionCount(1),
                              Gen2 = GC.CollectionCount(2),
                              Cpu = _cpu
                          };

            return metrics;
        }

        public void Dispose()
        {
            _process?.Dispose();
        }
    }
}
