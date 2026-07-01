using Microsoft.AspNetCore.Builder;
using global::Serilog;
using global::Serilog.Events;

namespace JorgeCostaMacia.Http.Serilog.Infrastructure;

/// <summary>
/// Configures Serilog's built-in HTTP request logging, emitting one summary log event per
/// request.
/// </summary>
internal static class RequestLoggingContext
{
    /// <summary>
    /// Adds <c>UserName</c> to the logged request-completion event, classifies the log level by
    /// status code and exception (<see cref="LogEventLevel.Error"/> for exceptions and 5xx
    /// responses, <see cref="LogEventLevel.Warning"/> for 4xx, <see cref="LogEventLevel.Information"/>
    /// otherwise), and sets the message template to <c>"Request End"</c>.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The same <see cref="WebApplication"/> instance, to allow method chaining.</returns>
    public static WebApplication Use(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("UserName", httpContext.User?.Identity?.Name ?? "anonymous");
            };

            options.GetLevel = (httpContext, elapsed, ex) =>
            {
                if (ex != null) return LogEventLevel.Error;
                if (httpContext.Response.StatusCode > 499) return LogEventLevel.Error;
                if (httpContext.Response.StatusCode > 399) return LogEventLevel.Warning;

                return LogEventLevel.Information;
            };

            options.MessageTemplate = "Request End";
        });

        return app;
    }
}
