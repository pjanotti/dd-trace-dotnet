using System.Reflection;
using AspNetCore31.Extensions;
using Datadog.Trace;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AspNetCore31
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            Tracer tracer = Tracer.Instance;
            string tracerVersion = tracer.GetType().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            services.AddDatadogTracing(tracer);
            services.AddDatadogRuntimeMetrics(tracer.DefaultServiceName, tracerVersion);

            /*
            services.AddSingleton<StatsdCounterSink>();

            services.AddSingleton<CounterMonitor>(provider =>
                                                  {
                                                      int processId;

                                                      using (var process = Process.GetCurrentProcess())
                                                      {
                                                          processId = process.Id;
                                                      }

                                                      var configuration = provider.GetService<IConfiguration>();
                                                      var serviceName = configuration["DD_SERVICE_NAME"];

                                                      var sink = provider.GetService<StatsdCounterSink>();
                                                      var counterMonitor = new CounterMonitor(processId, serviceName, sink);
                                                      counterMonitor.Start();
                                                      return counterMonitor;
                                                  });
            */
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IConfiguration configuration)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            if (configuration.GetValue<bool>("DD_DIAGNOSTIC_SOURCE_ENABLED"))
            {
                app.UseDatadogDiagnosticSource();
            }

            if (configuration.GetValue<bool>("DD_MIDDLEWARE_ENABLED"))
            {
                app.UseDatadogTracer();
            }

            app.Run(async context => { await context.Response.WriteAsync("Hello, world!"); });
        }
    }
}
