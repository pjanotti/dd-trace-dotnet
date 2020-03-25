namespace dotnet_metrics
{
    public struct CounterData
    {
        public int ProcessId;
        public string ProcessName;
        public string CounterName;
        public string CounterDisplayName;
        public CounterType Type;
        public double Value;

        public CounterData(int pid, string processName, string counterName, string counterDisplayName, CounterType type, double value)
        {
            ProcessId = pid;
            CounterName = counterName;
            CounterDisplayName = counterDisplayName;
            Type = type;
            Value = value;
            ProcessName = processName;
        }
    }
}
