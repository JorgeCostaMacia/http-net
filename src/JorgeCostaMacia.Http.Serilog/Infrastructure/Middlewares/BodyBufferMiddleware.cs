using Microsoft.AspNetCore.Http;

namespace JorgeCostaMacia.Http.Serilog.Infrastructure.Middlewares;

/// <summary>
/// Middleware that enables request body buffering so the body can be read more than once during the
/// request pipeline (e.g. by <see cref="EnrichRequestMiddleware"/> to log the body without consuming it
/// for the endpoint).
/// </summary>
public sealed class BodyBufferMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="BodyBufferMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public BodyBufferMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Enables buffering on the current request, then invokes the rest of the pipeline.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/>.</param>
    /// <returns>A task that completes when the rest of the pipeline has run.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();

        await _next(context);
    }
}
