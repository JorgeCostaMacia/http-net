using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace JorgeCostaMacia.Http.Serilog.Infrastructure;

/// <summary>
/// Enables request body buffering so the body can be read more than once during the request
/// pipeline.
/// </summary>
internal static class BodyBufferContext
{
    /// <summary>
    /// Enables buffering on every request, allowing downstream middleware (see
    /// <see cref="EnrichRequestContext"/>) to read the request body without consuming it for the
    /// endpoint.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The same <see cref="WebApplication"/> instance, to allow method chaining.</returns>
    public static WebApplication Use(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            context.Request.EnableBuffering();

            await next();
        });

        return app;
    }
}
