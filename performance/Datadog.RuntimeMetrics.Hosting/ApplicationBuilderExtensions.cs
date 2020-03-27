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
        public static IApplicationBuilder UseDatadogTracing(this IApplicationBuilder app, Tracer tracer, IOptions<TracingOptions> tracingOptions)
        {
            app.Use(async (context, next) =>
                    {
                        HttpRequest request = context.Request;
                        string httpMethod = request.Method?.ToUpperInvariant() ?? "UNKNOWN";
                        string url = GetUrl(request);
                        string resourceUrl = new Uri(url).AbsolutePath.ToLowerInvariant();
                        string resourceName = $"{httpMethod} {resourceUrl}";

                        using (Scope middlewareScope = tracer.StartActive("middleware"))
                        {
                            Span middlewareSpan = middlewareScope.Span;
                            middlewareSpan.Type = SpanTypes.Web;
                            middlewareSpan.ResourceName = resourceName?.Trim();
                            middlewareSpan.SetTag(Tags.SpanKind, SpanKinds.Server);
                            middlewareSpan.SetTag(Tags.HttpMethod, httpMethod);
                            middlewareSpan.SetTag(Tags.HttpRequestHeadersHost, request.Host.Value);
                            middlewareSpan.SetTag(Tags.HttpUrl, url);
                            middlewareSpan.SetTag(Tags.Language, "dotnet");

                            if (tracingOptions.Value.DD_MANUAL_SPANS_ENABLED)
                            {
                                using (Scope manualScope = tracer.StartActive("manual"))
                                {
                                    Span manualSpan = manualScope.Span;
                                    manualSpan.Type = SpanTypes.Custom;
                                    manualSpan.SetTag("tag1", "value1");
                                    manualSpan.SetTag("tag2", "value2");

                                    for (int i = 0; i < 5; i++)
                                    {
                                        using (Scope innerScope = tracer.StartActive("manual"))
                                        {
                                            Span innerSpan = innerScope.Span;
                                            innerSpan.Type = SpanTypes.Custom;
                                            manualSpan.SetTag("tag1", "value1");
                                            manualSpan.SetTag("tag2", "value2");
                                        }
                                    }
                                }
                            }

                            // call the next middleware in the chain
                            await next.Invoke();

                            middlewareSpan.SetTag(Tags.HttpStatusCode, context.Response.StatusCode.ToString(CultureInfo.InvariantCulture));
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
