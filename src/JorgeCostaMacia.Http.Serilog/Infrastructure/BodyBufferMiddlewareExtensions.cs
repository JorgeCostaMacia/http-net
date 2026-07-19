using JorgeCostaMacia.Http.Serilog.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace JorgeCostaMacia.Http.Serilog.Infrastructure;

/// <summary>Registers <see cref="BodyBufferMiddleware"/> in the request pipeline.</summary>
public static class BodyBufferMiddlewareExtensions
{
    /// <summary>
    /// Adds <see cref="BodyBufferMiddleware"/> to the request pipeline.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The <see cref="IApplicationBuilder"/>, to allow method chaining.</returns>
    public static IApplicationBuilder UseBodyBufferMiddleware(this WebApplication app) =>
        app.UseMiddleware<BodyBufferMiddleware>();
}
