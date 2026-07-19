using Serilog.AspNetCore;
using Serilog.Events;

namespace JorgeCostaMacia.Http.Serilog.Infrastructure;

/// <summary>
/// Extensions for <see cref="RequestLoggingOptions"/> that apply our default policy for Serilog's
/// built-in HTTP request logging (one summary log event per request). Kept as a
/// <see cref="RequestLoggingOptions"/> extension (not a hidden facade) so the framework's own
/// <c>UseSerilogRequestLogging</c> call stays visible in the host's <c>Program</c> while the policy lives
/// here.
/// </summary>
public static class RequestLoggingOptionsExtensions
{
    /// <summary>
    /// Adds <c>UserName</c> to the logged request-completion event, classifies the log level by status
    /// code and exception (<see cref="LogEventLevel.Error"/> for exceptions and 5xx responses,
    /// <see cref="LogEventLevel.Warning"/> for 4xx, <see cref="LogEventLevel.Information"/> otherwise), and
    /// sets the message template to <c>"Request End"</c>.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <returns>The same <paramref name="options"/>, for chaining.</returns>
    public static RequestLoggingOptions WithDefaults(this RequestLoggingOptions options)
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

        return options;
    }
}
