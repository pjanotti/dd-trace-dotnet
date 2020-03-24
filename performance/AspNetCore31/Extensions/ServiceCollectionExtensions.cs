using Datadog.RuntimeMetrics;
using Datadog.RuntimeMetrics.Hosting;
using Datadog.Trace;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StatsdClient;

namespace AspNetCore31.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatadogTracing(this IServiceCollection services, Tracer tracer = null)
        {
            Tracer.Instance = tracer ?? new Tracer();
            services.AddSingleton(Tracer.Instance);
            return services;
        }

        public static IServiceCollection AddDatadogRuntimeMetrics(this IServiceCollection services, string serviceName, string tracerVersion)
        {
            services.AddSingleton<IStatsd>(provider =>
                                           {
                                               string[] tags = null;

                                               if (!string.IsNullOrWhiteSpace(serviceName))
                                               {
                                                   tags = new[]
                                                          {
                                                              $"service_name:{serviceName}",
                                                              $"tracer_version:{tracerVersion}"
                                                          };
                                               }

                                               IConfiguration configuration = provider.GetService<IConfiguration>();
                                               string host = configuration.GetValue("DD_AGENT_HOST", "localhost");
                                               int port = configuration.GetValue("DD_DOGSTATSD_PORT", 8125);

                                               var statsd = new Statsd(new StatsdUDP(host, port),
                                                                       new RandomGenerator(),
                                                                       new StopWatchFactory(),
                                                                       prefix: string.Empty,
                                                                       tags);
                                               return statsd;
                                           });

            services.AddTransient<IRuntimeMetricsCollector, RuntimeMetricsCollector>();

            services.AddHostedService<RuntimeMetricsHostedService>(provider =>
                                                                   {
                                                                       IRuntimeMetricsCollector metricsCollector = provider.GetService<IRuntimeMetricsCollector>();
                                                                       var service = new RuntimeMetricsHostedService(metricsCollector);

                                                                       IStatsd statsd = provider.GetService<IStatsd>();
                                                                       var observer = new StatsdRuntimeMetricsObserver(statsd);

                                                                       service.Subscribe(observer);
                                                                       return service;
                                                                   });
            return services;
        }
    }
}
