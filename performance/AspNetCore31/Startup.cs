using System.Collections.Generic;
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
            services.Configure<StatsdConnectionOptions>(Configuration);

            // register the global Tracer
            services.AddDatadogTracing(Tracer.Instance);

            // register the services required to collect metrics and send them to dogstatsd
            services.AddDatadogRuntimeMetrics(options => options.Tags = GetMetricsTags());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Tracer tracer)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            int manualSpanCount = Configuration.GetValue("DD_MANUAL_SPAN_COUNT", 0);

            if (manualSpanCount > 0)
            {
                // hack: internal method
                tracer.GetType()
                      .GetMethod("StartDiagnosticObservers", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                     ?.Invoke(tracer, null);

                // first span is created with DiagnosticSource,
                // additional spans are created manually in middleware
                if (manualSpanCount > 1)
                {
                    app.UseDatadogTracing(tracer, manualSpanCount);
                }
            }

            app.Run(async context =>
                    {
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync("Hello, world!");
                    });
        }

        private IEnumerable<string> GetMetricsTags()
        {
            int manualSpanCount = Configuration.GetValue("DD_MANUAL_SPAN_COUNT", 0);
            string tracerVersion = Configuration["DD_TRACER_VERSION"];
            string[] tags = new string[4];

            tags[0] = "service_name:AspNetCore31";
            tags[1] = $"span_count:{manualSpanCount}";

            if (manualSpanCount == 0)
            {
                tags[2] = "tracer_mode:none";
                tags[3] = "tracer_version:none";
            }
            else
            {
                tags[2] = "tracer_mode:manual";
                tags[3] = $"tracer_version:{tracerVersion}";
            }

            return tags;
        }
    }
}
