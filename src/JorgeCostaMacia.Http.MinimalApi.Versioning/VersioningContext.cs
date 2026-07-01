using Asp.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JorgeCostaMacia.Http.MinimalApi.Versioning;

/// <summary>
/// Configures API Versioning for Minimal APIs using URL segments.
/// </summary>
public static class VersioningContext
{
    /// <summary>
    /// Reads the <c>ApiVersion</c> configuration setting and configures API Versioning using
    /// URL segments (e.g., <c>/v1/resource</c>).
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> containing the <c>ApiVersion</c> setting.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance, to allow method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <c>ApiVersion</c> is missing from configuration.</exception>
    /// <remarks>
    /// <para><b>Key features:</b></para>
    /// <list type="bullet">
    /// <item><description><b>Automatic Discovery:</b> Uses <c>ApiExplorer</c> to integrate with Swagger/OpenAPI.</description></item>
    /// <item><description><b>Header Reporting:</b> Adds <c>api-supported-versions</c> headers to responses.</description></item>
    /// <item><description><b>Substitution:</b> Automatically replaces the <c>{version:apiVersion}</c> token in route templates.</description></item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddVersioningContext(this IServiceCollection services, IConfiguration configuration)
    {
        int apiVersion = configuration.GetValue<int?>("ApiVersion") ?? throw new InvalidOperationException("'ApiVersion' is null.");

        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(apiVersion);
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(cfg =>
            {
                cfg.GroupNameFormat = "'v'V";
                cfg.SubstituteApiVersionInUrl = true;
            });

        return services;
    }
}
