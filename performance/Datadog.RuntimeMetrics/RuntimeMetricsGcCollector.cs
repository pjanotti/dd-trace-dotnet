using System;
using System.Diagnostics;

namespace Datadog.RuntimeMetrics
{
    // https://github.com/sebastienros/memoryleak/blob/master/src/MemoryLeak/MemoryLeak/Controllers/DiagnosticsController.cs

    public class RuntimeMetricsGcCollector : IDisposable, IRuntimeMetricsCollector
    {
        private readonly Process _process = Process.GetCurrentProcess();

        private TimeSpan _oldCpuTime = TimeSpan.Zero;
        private DateTime _lastMonitorTime = DateTime.UtcNow;
        private double _cpu;

        public GcMetrics GetRuntimeMetrics()
        {
            DateTime now = DateTime.UtcNow;
            _process.Refresh();

            double cpuElapsedTime = now.Subtract(_lastMonitorTime).TotalMilliseconds;

            TimeSpan newCpuTime = _process.TotalProcessorTime;
            double elapsedCpu = (newCpuTime - _oldCpuTime).TotalMilliseconds;
            _cpu = elapsedCpu * 100 / Environment.ProcessorCount / cpuElapsedTime;

            _lastMonitorTime = now;
            _oldCpuTime = newCpuTime;

            var metrics = new GcMetrics
                          {
                              Allocated = GC.GetTotalMemory(false),
                              WorkingSet = _process.WorkingSet64,
                              PrivateBytes = _process.PrivateMemorySize64,
                              GcCountGen0 = GC.CollectionCount(0),
                              GcCountGen1 = GC.CollectionCount(1),
                              GcCountGen2 = GC.CollectionCount(2),
                              CpuUsage = _cpu
                          };

            return metrics;
        }

        public void Dispose()
        {
            _process?.Dispose();
        }
    }
}
