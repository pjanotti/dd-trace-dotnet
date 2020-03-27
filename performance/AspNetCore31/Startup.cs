using System.Reflection;
using Datadog.RuntimeMetrics;
using Datadog.RuntimeMetrics.Hosting;
using Datadog.Trace;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AspNetCore31
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // configure options from default sources (e.g. env vars)
            services.Configure<TracingOptions>(Configuration);
            services.Configure<StatsdOptions>(Configuration);

            // register the global Tracer
            services.AddDatadogTracing();

            // register the services required to collect metrics and send them to dogstatsd
            services.AddDatadogRuntimeMetrics();

            services.AddMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Tracer tracer, IOptions<TracingOptions> tracingOptions)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            bool diagnosticSourceEnabled = tracingOptions.Value.DD_DIAGNOSTIC_SOURCE_ENABLED;
            bool middlewareEnabled = tracingOptions.Value.DD_MIDDLEWARE_ENABLED;
            bool manualSpansEnabled = tracingOptions.Value.DD_MANUAL_SPANS_ENABLED;

            if (diagnosticSourceEnabled)
            {
                // hack: internal method
                tracer.GetType()
                      .GetMethod("StartDiagnosticObservers", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                     ?.Invoke(tracer, null);
            }

            if (middlewareEnabled)
            {
                // if enabled, create a span for each request using middleware
                app.UseDatadogTracing(tracer, manualSpansEnabled);
            }

            app.Run(async context =>
                    {
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync("Hello, world!");
                    });
        }
    }
}
