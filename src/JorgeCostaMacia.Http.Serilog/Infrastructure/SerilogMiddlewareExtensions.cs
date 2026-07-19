using global::Serilog;
using global::Serilog.Context;
using global::Serilog.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace JorgeCostaMacia.Http.Serilog.Infrastructure;

/// <summary>
/// Extensions on <see cref="WebApplication"/> that register the Serilog request-logging pipeline:
/// request-body buffering, request- and authentication-level <see cref="LogContext"/> enrichment, and a
/// per-request summary log event. Each is a separate call so the host places it at the right point in its
/// own pipeline (e.g. authentication enrichment must run after <c>UseAuthentication</c>).
/// </summary>
public static class SerilogMiddlewareExtensions
{
    /// <summary>
    /// Enables request body buffering on every request, allowing downstream middleware
    /// (see <see cref="UseSerilogEnrichRequest"/>) to read the request body without consuming it for the
    /// endpoint.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The same <see cref="WebApplication"/> instance, to allow method chaining.</returns>
    public static WebApplication UseSerilogBodyBuffer(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            context.Request.EnableBuffering();

            await next();
        });

        return app;
    }

    /// <summary>
    /// Enriches the Serilog <see cref="LogContext"/> with request-level diagnostic data for the duration of
    /// the request: scheme, host, client IP, content type, query string, body, user agent, and the
    /// <c>X-Request-ID</c> header.
    /// </summary>
    /// <remarks>
    /// The request body is only read when the method is POST, PUT, or PATCH, a positive <c>ContentLength</c>
    /// is present, and the stream is seekable (i.e. after <see cref="UseSerilogBodyBuffer"/> has run earlier
    /// in the pipeline). Reading the body resets its position back to zero afterwards so downstream
    /// middleware and the endpoint can still read it; if reading fails for any reason, the body is reported
    /// as <c>"[Error reading body]"</c> instead of throwing.
    /// </remarks>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The same <see cref="WebApplication"/> instance, to allow method chaining.</returns>
    public static WebApplication UseSerilogEnrichRequest(this WebApplication app)
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
                await next();
            }
        });

        return app;
    }

    /// <summary>
    /// Enriches the Serilog <see cref="LogContext"/> with the authenticated user's name for the duration of
    /// the request, falling back to <c>"anonymous"</c> when no user is authenticated.
    /// </summary>
    /// <remarks>
    /// Must be registered after authentication middleware (e.g. <c>app.UseAuthentication()</c>) so the user
    /// is already populated on the request. Only enriches the ambient log context for events emitted after
    /// this point; the final Serilog request-completion event is enriched separately by
    /// <see cref="UseSerilogRequestSummary"/>, since by the time that event is logged this middleware's
    /// scope has already closed.
    /// </remarks>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The same <see cref="WebApplication"/> instance, to allow method chaining.</returns>
    public static WebApplication UseSerilogEnrichAuthentication(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            using (LogContext.PushProperty("UserName", context.User?.Identity?.Name ?? "anonymous"))
            {
                await next();
            }
        });

        return app;
    }

    /// <summary>
    /// Configures Serilog's built-in HTTP request logging, emitting one summary log event per request.
    /// Adds <c>UserName</c> to the logged request-completion event, classifies the log level by status code
    /// and exception (<see cref="LogEventLevel.Error"/> for exceptions and 5xx responses,
    /// <see cref="LogEventLevel.Warning"/> for 4xx, <see cref="LogEventLevel.Information"/> otherwise), and
    /// sets the message template to <c>"Request End"</c>.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The same <see cref="WebApplication"/> instance, to allow method chaining.</returns>
    public static WebApplication UseSerilogRequestSummary(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) => diagnosticContext.Set("UserName", httpContext.User?.Identity?.Name ?? "anonymous");

            options.GetLevel = (httpContext, elapsed, ex) =>
            {
                if (ex != null)
                {
                    return LogEventLevel.Error;
                }

                if (httpContext.Response.StatusCode > 499)
                {
                    return LogEventLevel.Error;
                }

                if (httpContext.Response.StatusCode > 399)
                {
                    return LogEventLevel.Warning;
                }

                return LogEventLevel.Information;
            };

            options.MessageTemplate = "Request End";
        });

        return app;
    }
}
