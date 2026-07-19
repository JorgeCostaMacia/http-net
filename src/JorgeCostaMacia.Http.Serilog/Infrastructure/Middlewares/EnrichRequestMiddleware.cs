using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace JorgeCostaMacia.Http.Serilog.Infrastructure.Middlewares;

/// <summary>
/// Middleware that enriches the Serilog <see cref="LogContext"/> with request-level diagnostic data for
/// the duration of the request: scheme, host, client IP, content type, query string, body, user agent,
/// and the <c>X-Request-ID</c> header.
/// </summary>
/// <remarks>
/// The request body is only read when the method is POST, PUT, or PATCH, a positive <c>ContentLength</c>
/// is present, and the stream is seekable (i.e. after <see cref="BodyBufferMiddleware"/> has run earlier
/// in the pipeline). Reading the body resets its position back to zero afterwards so downstream
/// middleware and the endpoint can still read it; if reading fails for any reason, the body is reported
/// as <c>"[Error reading body]"</c> instead of throwing.
/// </remarks>
public sealed class EnrichRequestMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnrichRequestMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public EnrichRequestMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Reads the request body (when applicable), pushes the request diagnostic properties onto the
    /// <see cref="LogContext"/>, then invokes the rest of the pipeline within that scope.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/>.</param>
    /// <returns>A task that completes when the rest of the pipeline has run.</returns>
    public async Task InvokeAsync(HttpContext context)
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
            }
            catch { body = "[Error reading body]"; }
            finally
            {
                // always rewind — a read that failed mid-stream must not leave the endpoint's
                // model binding starting from wherever the read stopped.
                context.Request.Body.Position = 0;
            }
        }

        using (LogContext.PushProperty("RequestScheme", context.Request.Scheme))
        using (LogContext.PushProperty("RequestHost", context.Request.Host.Value))
        using (LogContext.PushProperty("RequestIp", context.Connection.RemoteIpAddress?.ToString() ?? "unknown"))
        using (LogContext.PushProperty("RequestContentType", context.Request.ContentType ?? string.Empty))
        using (LogContext.PushProperty("RequestQueryString", context.Request.QueryString.Value ?? string.Empty))
        using (LogContext.PushProperty("RequestBody", body))
        using (LogContext.PushProperty("UserAgent", context.Request.Headers.TryGetValue("User-Agent", out StringValues userAgent) ? !string.IsNullOrEmpty(userAgent) ? userAgent.ToString() : "unknown" : "unknown"))
        using (LogContext.PushProperty("XRequestId", context.Request.Headers.TryGetValue("X-Request-ID", out StringValues xRequestId) ? !string.IsNullOrEmpty(xRequestId) ? xRequestId.ToString() : "unknown" : "unknown"))
        {
            await _next(context);
        }
    }
}
