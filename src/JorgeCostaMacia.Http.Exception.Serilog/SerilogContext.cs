using Microsoft.Extensions.DependencyInjection;
using JorgeCostaMacia.Http.Exception.Serilog.Infrastructure;

namespace JorgeCostaMacia.Http.Exception.Serilog;

/// <summary>
/// Registers Serilog-based logging for unhandled exceptions.
/// </summary>
public static class SerilogContext
{
    /// <summary>
    /// Registers <see cref="ExceptionHandler"/>, which logs every unhandled exception without
    /// producing the response itself — that remains the responsibility of
    /// <c>ExceptionContext</c>'s status code mapping.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance, to allow method chaining.</returns>
    public static IServiceCollection AddExceptionHandlerContext(this IServiceCollection services)
    {
        services.AddExceptionHandler<ExceptionHandler>();

        return services;
    }
}
