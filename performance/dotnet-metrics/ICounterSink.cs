namespace dotnet_metrics
{
    public interface ICounterSink
    {
        void OnCounterUpdate(CounterEventArgs counterEventArgs);
    }
}
