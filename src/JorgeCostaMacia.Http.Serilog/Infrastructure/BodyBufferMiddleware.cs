using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace JorgeCostaMacia.Http.Serilog.Infrastructure;

/// <summary>
/// Middleware that enables request body buffering so the body can be read more than once during the
/// request pipeline.
/// </summary>
public static class BodyBufferMiddleware
{
    /// <summary>
    /// Enables buffering on every request, allowing downstream middleware (see
    /// <see cref="EnrichRequestMiddleware"/>) to read the request body without consuming it for the
    /// endpoint.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The same <see cref="WebApplication"/> instance, to allow method chaining.</returns>
    public static WebApplication Use(WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            context.Request.EnableBuffering();

            await next();
        });

        return app;
    }
}
