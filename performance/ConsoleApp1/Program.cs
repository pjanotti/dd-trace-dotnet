using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.Diagnostics.Tracing.Session;

namespace ConsoleApp1
{
    class Program
    {
        static void Main()
        {
            int processId;

            using (var process = Process.GetCurrentProcess())
            {
                processId = process.Id;
            }

            using (var session = new TraceEventSession("MyRealTimeSession")) // Create a session to listen for events
            {
                Console.CancelKeyPress += (sender, args) =>
                                          {
                                              Console.WriteLine("Exiting...");
                                              session.Dispose();
                                          };

                byte[] bytes;

                Task.Run(() =>
                         {
                             while (true)
                             {
                                 bytes = new byte[1024];
                                 Thread.Sleep(10);
                             }
                         });

                session.Source.Clr.GCTriggered += data =>
                                                  {
                                                      if (data.ProcessID == processId)
                                                      {
                                                          Console.WriteLine(data);
                                                      }
                                                  };

                /*
                session.Source.Clr.GCHeapStats += data =>
                                                  {
                                                      if (data.ProcessID == processId)
                                                      {
                                                          Console.WriteLine(data);
                                                      }
                                                  };
                */

                session.EnableProvider(ClrTraceEventParser.ProviderGuid);
                session.Source.Process();
            }
        }
    }
}
