namespace Datadog.RuntimeMetrics
{
    public struct RuntimeMetrics
    {
        public long Allocated;
        public long WorkingSet;
        public long PrivateBytes;
        public int Gen0;
        public int Gen1;
        public int Gen2;
        public double Cpu;
    }
}
