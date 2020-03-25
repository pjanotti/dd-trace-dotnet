using System;

namespace dotnet_metrics
{
    public class ConsoleShipper : IObserver<CounterData>
    {
        /// <summary>Notifies the observer that the provider has finished sending push-based notifications.</summary>
        public void OnCompleted()
        {
        }

        /// <summary>Notifies the observer that the provider has experienced an error condition.</summary>
        /// <param name="error">An object that provides additional information about the error.</param>
        public void OnError(Exception error)
        {
            Console.WriteLine(error.ToString());
        }

        /// <summary>Provides the observer with new data.</summary>
        /// <param name="value">The current notification information.</param>
        public void OnNext(CounterData value)
        {
            Console.WriteLine($"{value.ProcessId,7} {value.CounterName,-34}{value.CounterDisplayName,-40}{value.Value,10:N0}");
        }
    }
}
