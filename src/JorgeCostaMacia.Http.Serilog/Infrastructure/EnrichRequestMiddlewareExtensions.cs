using JorgeCostaMacia.Http.Serilog.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace JorgeCostaMacia.Http.Serilog.Infrastructure;

/// <summary>Registers <see cref="EnrichRequestMiddleware"/> in the request pipeline.</summary>
public static class EnrichRequestMiddlewareExtensions
{
    /// <summary>
    /// Adds <see cref="EnrichRequestMiddleware"/> to the request pipeline.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The <see cref="IApplicationBuilder"/>, to allow method chaining.</returns>
    public static IApplicationBuilder UseEnrichRequestMiddleware(this WebApplication app) =>
        app.UseMiddleware<EnrichRequestMiddleware>();
}
