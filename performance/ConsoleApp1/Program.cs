using System;
using System.Threading.Tasks;
using Datadog.RuntimeMetrics;

namespace ConsoleApp1
{
    public class Program
    {
        public static async Task Main()
        {
            var listener = new SimpleEventListener();

            await Task.Delay(TimeSpan.FromSeconds(5));

        }
    }
}
