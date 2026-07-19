using JorgeCostaMacia.Http.Serilog.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace JorgeCostaMacia.Http.Serilog.Infrastructure;

/// <summary>Registers <see cref="EnrichAuthenticationMiddleware"/> in the request pipeline.</summary>
public static class EnrichAuthenticationMiddlewareExtensions
{
    /// <summary>
    /// Adds <see cref="EnrichAuthenticationMiddleware"/> to the request pipeline.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The <see cref="IApplicationBuilder"/>, to allow method chaining.</returns>
    public static IApplicationBuilder UseEnrichAuthenticationMiddleware(this WebApplication app) =>
        app.UseMiddleware<EnrichAuthenticationMiddleware>();
}
