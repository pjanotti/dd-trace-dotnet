using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Datadog.Trace;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StatsdClient;

namespace Datadog.RuntimeMetrics.Hosting
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatadogTracing(this IServiceCollection services, Tracer tracer = null)
        {
            services.AddSingleton(provider =>
                                  {
                                      if (tracer == null)
                                      {
                                          tracer = Tracer.Instance;
                                      }
                                      else
                                      {
                                          Tracer.Instance = tracer;
                                      }

                                      IConfiguration configuration = provider.GetService<IConfiguration>();

                                      if (configuration.GetValue("DD_DIAGNOSTIC_SOURCE_ENABLED", defaultValue: false))
                                      {
                                          tracer.GetType()
                                                .GetMethod("StartDiagnosticObservers", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                               ?.Invoke(tracer, null);
                                      }

                                      return tracer;
                                  });

            return services;
        }

        public static IServiceCollection AddDatadogRuntimeMetrics(this IServiceCollection services, IEnumerable<string> customTags = null)
        {
            services.AddTransient<IRandomGenerator, RandomGenerator>();

            services.AddTransient<IStopWatchFactory, StopWatchFactory>();

            services.AddTransient<IStatsdUDP>(provider =>
                                              {
                                                  IConfiguration configuration = provider.GetService<IConfiguration>();
                                                  string host = configuration.GetValue("DD_AGENT_HOST", "localhost");
                                                  int port = configuration.GetValue("DD_DOGSTATSD_PORT", 8125);
                                                  return new StatsdUDP(host, port);
                                              });

            services.AddSingleton<IStatsd>(provider =>
                                           {
                                               IConfiguration configuration = provider.GetService<IConfiguration>();
                                               bool diagnosticSourceEnabled = configuration.GetValue("DD_DIAGNOSTIC_SOURCE_ENABLED", false);
                                               bool middlewareEnabled = configuration.GetValue("DD_MIDDLEWARE_ENABLED", false);
                                               string tracerVersion = configuration.GetValue("DD_TRACER_VERSION", "latest");

                                               Tracer tracer = provider.GetService<Tracer>();

                                               var internalTags = new List<string>
                                                                  {
                                                                      $"service_name:{tracer.DefaultServiceName}"
                                                                  };

                                               if (diagnosticSourceEnabled)
                                               {
                                                   internalTags.Add("tracer_mode:diagnostic-source");
                                                   internalTags.Add($"tracer_version:{tracerVersion}");
                                               }
                                               else if (middlewareEnabled)
                                               {
                                                   internalTags.Add("tracer_mode:middleware");
                                                   internalTags.Add($"tracer_version:{tracerVersion}");
                                               }
                                               else
                                               {
                                                   internalTags.Add("tracer_mode:none");
                                                   internalTags.Add("tracer_version:none");
                                               }

                                               if (customTags == null)
                                               {
                                                   customTags = Enumerable.Empty<string>();
                                               }

                                               var statsd = new Statsd(provider.GetService<IStatsdUDP>(),
                                                                       provider.GetService<IRandomGenerator>(),
                                                                       provider.GetService<IStopWatchFactory>(),
                                                                       prefix: string.Empty,
                                                                       internalTags.Concat(customTags).ToArray());
                                               return statsd;
                                           });

            services.AddTransient<IRuntimeMetricsCollector, RuntimeMetricsCollector>();

            services.AddTransient<IObserver<RuntimeMetrics>, StatsdRuntimeMetricsObserver>();

            services.AddTransient(provider =>
                                  {
                                      var metricsCollector = provider.GetService<IRuntimeMetricsCollector>();
                                      var service = new RuntimeMetricsService(metricsCollector);

                                      var observer = provider.GetService<IObserver<RuntimeMetrics>>();

                                      service.Subscribe(observer);
                                      return service;
                                  });

            services.AddHostedService<RuntimeMetricsHostedService>();
            return services;
        }
    }
}
