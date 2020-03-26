using System;
using System.Globalization;
using Datadog.Trace;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Datadog.RuntimeMetrics.Hosting
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds Datadog tracing middleware.
        /// </summary>
        public static IApplicationBuilder UseDatadogTracing(this IApplicationBuilder app, Tracer tracer)
        {
            app.Use(async (context, next) =>
                    {
                        HttpRequest request = context.Request;
                        string httpMethod = request.Method?.ToUpperInvariant() ?? "UNKNOWN";
                        string url = GetUrl(request);
                        string resourceUrl = new Uri(url).AbsolutePath.ToLowerInvariant();
                        string resourceName = $"{httpMethod} {resourceUrl}";

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

                            // call the next middleware in the chain
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
