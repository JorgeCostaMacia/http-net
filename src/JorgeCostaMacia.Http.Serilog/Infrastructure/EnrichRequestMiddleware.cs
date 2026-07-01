using global::Serilog.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace JorgeCostaMacia.Http.Serilog.Infrastructure;

/// <summary>
/// Enriches the Serilog <see cref="LogContext"/> with request-level diagnostic data for the
/// duration of the request: scheme, host, client IP, content type, query string, body, user
/// agent, and the <c>X-Request-ID</c> header.
/// </summary>
/// <remarks>
/// The request body is only read when the method is POST, PUT, or PATCH, a positive
/// <c>ContentLength</c> is present, and the stream is seekable (i.e. after
/// <see cref="BodyBufferContext"/> has run earlier in the pipeline). Reading the body resets its
/// position back to zero afterwards so downstream middleware and the endpoint can still read
/// it; if reading fails for any reason, the body is reported as <c>"[Error reading body]"</c>
/// instead of throwing.
/// </remarks>
internal static class EnrichRequestContext
{
    /// <summary>
    /// Registers the middleware described in the type-level remarks.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The same <see cref="WebApplication"/> instance, to allow method chaining.</returns>
    public static WebApplication Use(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            string body = string.Empty;

            if ((HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method) || HttpMethods.IsPatch(context.Request.Method)) && context.Request.ContentLength > 0 && context.Request.Body.CanSeek)
            {
                try
                {
                    context.Request.Body.Position = 0;
                    using (StreamReader reader = new StreamReader(context.Request.Body, leaveOpen: true))
                    {
                        body = await reader.ReadToEndAsync();
                    }
                    context.Request.Body.Position = 0;
                }
                catch { body = "[Error reading body]"; }
            }

            using (LogContext.PushProperty("RequestScheme", context.Request.Scheme))
            using (LogContext.PushProperty("RequestHost", context.Request.Host.Value))
            using (LogContext.PushProperty("RequestIp", context.Connection.RemoteIpAddress?.ToString() ?? "unknown"))
            using (LogContext.PushProperty("RequestContentType", context.Request.ContentType ?? string.Empty))
            using (LogContext.PushProperty("RequestQueryString", context.Request.QueryString.Value ?? string.Empty))
            using (LogContext.PushProperty("RequestBody", body))
            using (LogContext.PushProperty("UserAgent", context.Request.Headers.TryGetValue("User-Agent", out var userAgent) ? !string.IsNullOrEmpty(userAgent) ? userAgent.ToString() : "unknown" : "unknown"))
            using (LogContext.PushProperty("XRequestId", context.Request.Headers.TryGetValue("X-Request-ID", out var xRequestId) ? !string.IsNullOrEmpty(xRequestId) ? xRequestId.ToString() : "unknown" : "unknown"))
            {
                await next();
            }
        });

        return app;
    }
}
