using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Datadog.Trace;
using Datadog.Trace.Agent;
using Datadog.Trace.ExtensionMethods;
using MessagePack;
using MsgPack.Serialization;

namespace MsgPack.Benchmarks
{
    [MemoryDiagnoser]
    [GcServer(true)]
    [GcForce(false)]
    public class Benchmarks
    {
        private const int InitialBufferSize = 3 * 1024 * 1024;

        [Params(1, 300)]
        public int TraceCount { get; set; }

        [Params(1, 10)]
        public int SpansPerTrace { get; set; }

        private readonly MemoryStream _stream1 = new MemoryStream(InitialBufferSize);
        private readonly MemoryStream _stream2 = new MemoryStream(InitialBufferSize);
        private readonly ArrayBufferWriter<byte> _bufferWriter = new ArrayBufferWriter<byte>(InitialBufferSize);

        private SerializationContext _msgPackSerializationContext;
        private SpanMessagePackSerializer _msgPackSerializer;
        private Span[][] _traces;
        private MockSpan[][] _mockTraces;

        [GlobalSetup]
        public void SetupTraces()
        {
            _msgPackSerializationContext = new SerializationContext();
            _msgPackSerializer = new SpanMessagePackSerializer(_msgPackSerializationContext);

            _msgPackSerializationContext.ResolveSerializer += (sender, eventArgs) =>
                                                              {
                                                                  if (eventArgs.TargetType == typeof(Span))
                                                                  {
                                                                      eventArgs.SetSerializer(_msgPackSerializer);
                                                                  }
                                                              };

            _traces = new Span[TraceCount][];

            for (ulong traceIndex = 0; traceIndex < (ulong)_traces.Length; traceIndex++)
            {
                var trace = new Span[SpansPerTrace];

                var rootSpan = new Span(traceId: traceIndex + 1,
                                        spanId: 1,
                                        error: false,
                                        operationName: "root",
                                        resourceName: "resource",
                                        serviceName: "service",
                                        spanType: "custom",
                                        startTime: DateTimeOffset.UtcNow,
                                        duration: TimeSpan.FromSeconds(1));

                for (int tagIndex = 0; tagIndex < 10; tagIndex++)
                {
                    rootSpan.SetTag($"key{tagIndex}", $"value{tagIndex}");
                }

                rootSpan.SetTraceSamplingPriority(SamplingPriority.UserReject);
                trace[0] = rootSpan;

                for (ulong spanIndex = 1; spanIndex < (ulong)trace.Length; spanIndex++)
                {
                    var childSpan = new Span(traceId: rootSpan.TraceId,
                                             spanId: spanIndex + 1,
                                             error: false,
                                             operationName: "child",
                                             resourceName: "resource",
                                             serviceName: "service",
                                             spanType: "custom",
                                             startTime: DateTimeOffset.UtcNow,
                                             duration: TimeSpan.FromSeconds(1));

                    for (int tagIndex = 0; tagIndex < 5; tagIndex++)
                    {
                        childSpan.SetTag($"key{tagIndex}", $"value{tagIndex}");
                    }

                    trace[spanIndex] = childSpan;
                }

                _traces[traceIndex] = trace;
            }

            _mockTraces = new MockSpan[_traces.Length][];

            // copy Spans to MockSpans
            for (ulong traceIndex = 0; traceIndex < (ulong)_traces.Length; traceIndex++)
            {
                Span[] trace = _traces[traceIndex];
                var mockTrace = new MockSpan[trace.Length];
                _mockTraces[traceIndex] = mockTrace;

                for (ulong spanIndex = 0; spanIndex < (ulong)trace.Length; spanIndex++)
                {
                    var span = trace[spanIndex];

                    var mockSpan = new MockSpan
                                   {
                                       TraceId = span.TraceId,
                                       SpanId = span.SpanId,
                                       Error = (byte)(span.Error ? 1 : 0),
                                       Operation = span.OperationName,
                                       Resource = span.ResourceName,
                                       Service = span.ServiceName,
                                       Type = span.Type,
                                       Start = span.StartTime.ToUnixTimeNanoseconds(),
                                       Duration = span.Duration.ToNanoseconds(),
                                       Tags = span.Tags,
                                       Metrics = span.Metrics
                                   };

                    mockTrace[spanIndex] = mockSpan;
                }
            }
        }

        [Benchmark]
        public Task MsgPackCli_MsgPackContent_NullStream()
        {
            _stream1.Position = 0;
            var content = new MsgPackContent<Span[][]>(_traces, _msgPackSerializationContext);
            return content.CopyToAsync(_stream1);
        }

        [Benchmark]
        public Task MessagePack_MessagePackSerializer_NullStream()
        {
            _stream2.Position = 0;
            return global::MessagePack.MessagePackSerializer.SerializeAsync(_stream2, _mockTraces);
        }

        [Benchmark]
        public void MessagePack_MessagePackWriter()
        {
            _bufferWriter.Clear();
            Serialize(_mockTraces, _bufferWriter);
        }

        public static void Serialize(MockSpan[][] traces, IBufferWriter<byte> bufferWriter)
        {
            var writer = new MessagePackWriter(bufferWriter);

            writer.WriteArrayHeader(traces.Length);

            foreach (MockSpan[] trace in traces)
            {
                writer.WriteArrayHeader(trace.Length);

                foreach (MockSpan span in trace)
                {
                    int length = 8;

                    if (span.ParentId != null)
                    {
                        length++;
                    }

                    if (span.Error == 1)
                    {
                        length++;
                    }

                    if (span.Tags?.Count > 0)
                    {
                        length++;
                    }

                    if (span.Metrics?.Count > 0)
                    {
                        length++;
                    }

                    writer.WriteMapHeader(length);
                    writer.Write("trace_id");
                    writer.Write(span.TraceId);
                    writer.Write("span_id");
                    writer.Write(span.SpanId);
                    writer.Write("name");
                    writer.Write(span.Operation);
                    writer.Write("resource");
                    writer.Write(span.Resource);
                    writer.Write("service");
                    writer.Write(span.Service);
                    writer.Write("type");
                    writer.Write(span.Type);
                    writer.Write("start");
                    writer.Write(span.Start);
                    writer.Write("duration");
                    writer.Write(span.Duration);

                    if (span.ParentId != null)
                    {
                        writer.Write("parent_id");
                        writer.Write((ulong)span.ParentId);
                    }

                    if (span.Error == 1)
                    {
                        writer.Write("error");
                        writer.Write(1);
                    }

                    if (span.Tags?.Count > 0)
                    {
                        writer.Write("meta");
                        writer.WriteMapHeader(span.Tags.Count);

                        foreach (var pair in span.Tags)
                        {
                            writer.Write(pair.Key);
                            writer.Write(pair.Value);
                        }
                    }

                    if (span.Metrics?.Count > 0)
                    {
                        writer.Write("metrics");
                        writer.WriteMapHeader(span.Metrics.Count);

                        foreach (var pair in span.Metrics)
                        {
                            writer.Write(pair.Key);
                            writer.Write(pair.Value);
                        }
                    }
                }
            }

            writer.Flush();
        }
    }
}
