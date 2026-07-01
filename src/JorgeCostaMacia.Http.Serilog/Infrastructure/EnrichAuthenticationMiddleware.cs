using Microsoft.AspNetCore.Builder;
using global::Serilog.Context;

namespace JorgeCostaMacia.Http.Serilog.Infrastructure;

/// <summary>
/// Enriches the Serilog <see cref="LogContext"/> with the authenticated user's name for the
/// duration of the request, falling back to <c>"anonymous"</c> when no user is authenticated.
/// </summary>
/// <remarks>
/// Must be registered after authentication middleware (e.g. <c>app.UseAuthentication()</c>) so
/// that the user is already populated on the request. Only enriches the ambient log context for
/// log events emitted after this point; the final Serilog request-completion event is enriched
/// separately by <see cref="RequestLoggingContext"/>, since by the time that event is logged
/// this middleware's scope has already closed.
/// </remarks>
internal static class EnrichAuthenticationContext
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
            using (LogContext.PushProperty("UserName", context.User?.Identity?.Name ?? "anonymous"))
            {
                await next();
            }
        });

        return app;
    }
}
