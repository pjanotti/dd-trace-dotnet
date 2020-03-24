using System;
using System.Globalization;
using System.Reflection;
using Datadog.RuntimeMetrics;
using Datadog.RuntimeMetrics.Hosting;
using Datadog.Trace;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using StatsdClient;

namespace AspNetCore31.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseDatadogDiagnosticSource(this IApplicationBuilder app)
        {
            Tracer tracer = app.ApplicationServices.GetService<Tracer>();

            tracer.GetType()
                  .GetMethod("StartDiagnosticObservers", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                 ?.Invoke(tracer, null);

            return app;
        }

        public static IApplicationBuilder UseDatadogTracer(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
                    {
                        HttpRequest request = context.Request;
                        string httpMethod = request.Method?.ToUpperInvariant() ?? "UNKNOWN";
                        string url = GetUrl(request);
                        string resourceUrl = new Uri(url).AbsolutePath.ToLowerInvariant();
                        string resourceName = $"{httpMethod} {resourceUrl}";

                        Tracer tracer = app.ApplicationServices.GetService<Tracer>();

                        using (Scope scope = tracer.StartActive("aspnet_core.middleware"))
                        {
                            Span span = scope.Span;
                            span.Type = SpanTypes.Web;
                            span.ResourceName = resourceName?.Trim();
                            span.SetTag(Tags.SpanKind, SpanKinds.Server);
                            span.SetTag(Tags.HttpMethod, httpMethod);
                            span.SetTag(Tags.HttpRequestHeadersHost, request.Host.Value);
                            span.SetTag(Tags.HttpUrl, url);
                            span.SetTag(Tags.Language, "dotnet");

                            await next.Invoke();

                            span.SetTag(Tags.HttpStatusCode, context.Response.StatusCode.ToString(CultureInfo.InvariantCulture));
                        }
                    });


            return app;
        }

        private static string GetUrl(HttpRequest request)
        {
            if (request.Host.HasValue)
            {
                return $"{request.Scheme}://{request.Host.Value}{request.PathBase.Value}{request.Path.Value}";
            }

            // HTTP 1.0 requests are not required to provide a Host to be valid
            // Since this is just for display, we can provide a string that is
            // not an actual Uri with only the fields that are specified.
            // request.GetDisplayUrl(), used above, will throw an exception
            // if request.Host is null.
            return $"{request.Scheme}://UNKNOWN_HOST{request.PathBase.Value}{request.Path.Value}";
        }
    }
}
