using Microsoft.AspNetCore.Builder;
using JorgeCostaMacia.Http.Serilog.Infrastructure;

namespace JorgeCostaMacia.Http.Serilog;

/// <summary>
/// Registers Serilog-related middleware for request body buffering, diagnostic enrichment, and
/// HTTP request logging.
/// </summary>
public static class SerilogContext
{
    /// <summary>
    /// Enables request body buffering. See <see cref="BodyBufferContext"/>.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The same <see cref="WebApplication"/> instance, to allow method chaining.</returns>
    public static WebApplication UseSerilogBodyBufferContext(this WebApplication app) => BodyBufferContext.Use(app);

    /// <summary>
    /// Enriches the log context with request-level diagnostic data. See <see cref="EnrichRequestContext"/>.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The same <see cref="WebApplication"/> instance, to allow method chaining.</returns>
    public static WebApplication UseSerilogEnrichRequestContext(this WebApplication app) => EnrichRequestContext.Use(app);

    /// <summary>
    /// Enriches the log context with the authenticated user's name. See <see cref="EnrichAuthenticationContext"/>.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The same <see cref="WebApplication"/> instance, to allow method chaining.</returns>
    public static WebApplication UseSerilogEnrichAuthenticationContext(this WebApplication app) => EnrichAuthenticationContext.Use(app);

    /// <summary>
    /// Configures Serilog's HTTP request logging. See <see cref="RequestLoggingContext"/>.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The same <see cref="WebApplication"/> instance, to allow method chaining.</returns>
    public static WebApplication UseSerilogRequestLoggingContext(this WebApplication app) => RequestLoggingContext.Use(app);
}
