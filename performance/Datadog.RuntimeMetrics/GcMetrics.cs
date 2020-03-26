namespace Datadog.RuntimeMetrics
{
    public struct GcMetrics
    {
        public long Allocated;
        public long WorkingSet;
        public long PrivateBytes;
        public int GcCountGen0;
        public int GcCountGen1;
        public int GcCountGen2;
        public double CpuUsage;
    }
}
