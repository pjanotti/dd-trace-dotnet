using System;
using System.Collections.Generic;
using System.Linq;

namespace Datadog.MockTraceAgent
{
    public class Program
    {
        public static void Main()
        {
            using (var agent = new TraceAgent())
            {
                agent.RequestDeserialized += TracesReceived;

                int port = agent.Start();

                Console.WriteLine($"Listening on http://localhost:{port}");
                Console.WriteLine("Press ENTER or CTRL+C to exit.");
                Console.ReadLine();
            }

            Environment.Exit(0);
        }

        private static void TracesReceived(object sender, EventArgs<IList<IList<MockSpan>>> traces)
        {
            int traceCount = traces.Value.Count;
            int spanCount = traces.Value.SelectMany(t => t).Count();

            Console.WriteLine($"{traceCount} traces received with {spanCount} spans.");

            /*
            foreach (IList<Span> trace in traces.Value)
            {
                foreach (Span span in trace)
                {
                    Console.WriteLine($"TraceId={span.TraceId}, SpanId={span.SpanId}, Service={span.Service}, Name={span.Name}, Resource={span.Resource}");
                }
            }
            */
        }
    }
}
