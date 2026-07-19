using global::Serilog.Context;
using Microsoft.AspNetCore.Http;

namespace JorgeCostaMacia.Http.Serilog.Infrastructure.Middlewares;

/// <summary>
/// Middleware that enriches the Serilog <see cref="LogContext"/> with the authenticated user's name for
/// the duration of the request, falling back to <c>"anonymous"</c> when no user is authenticated.
/// </summary>
/// <remarks>
/// Must be registered after authentication middleware (e.g. <c>app.UseAuthentication()</c>) so the user
/// is already populated on the request. Only enriches the ambient log context for events emitted after
/// this point; the final Serilog request-completion event is enriched separately by the request-logging
/// options (see <c>RequestLoggingOptionsExtensions</c>), since by the time that event is logged this
/// middleware's scope has already closed.
/// </remarks>
public sealed class EnrichAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnrichAuthenticationMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public EnrichAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Pushes the authenticated user's name onto the <see cref="LogContext"/>, then invokes the rest of
    /// the pipeline within that scope.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/>.</param>
    /// <returns>A task that completes when the rest of the pipeline has run.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        using (LogContext.PushProperty("UserName", context.User?.Identity?.Name ?? "anonymous"))
        {
            await _next(context);
        }
    }
}
