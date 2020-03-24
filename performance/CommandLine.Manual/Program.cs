using System.Threading;
using System.Threading.Tasks;
using Datadog.Trace;

namespace CommandLine.Manual
{
    internal class Program
    {
        const int threads = 8;
        const int rootSpanCount = 20;
        const int childSpanCount = 30;
        const int childSpanPadding = 10;
        const int childSpanDuration = 20;

        private static async Task Main()
        {
            var tasks = new Task[threads];

            for (int i = 0; i < threads; i++)
            {
                tasks[i] = Task.Run(CreateTraces);
            }

            await Task.WhenAll(tasks);
            //await Tracer.Instance.FlushAsync();
        }

        private static void CreateTraces()
        {
            for (int i = 0; i < rootSpanCount; i++)
            {
                CreateTrace();
            }
        }

        private static void CreateTrace()
        {
            using (var rootScope = Tracer.Instance.StartActive("root"))
            {
                for (int childIndex = 0; childIndex < childSpanCount; childIndex++)
                {
                    using (var childScope = Tracer.Instance.StartActive("child"))
                    {
                        Thread.Sleep(childSpanDuration);
                    }

                    Thread.Sleep(childSpanPadding);
                }
            }
        }
    }
}
