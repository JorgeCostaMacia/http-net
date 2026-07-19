using JorgeCostaMacia.Http.OpenApi.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace JorgeCostaMacia.Http.OpenApi;

/// <summary>
/// Configures native .NET OpenAPI generation with RFC 7807 Problem Details schema enrichment.
/// </summary>
public static class OpenApiContext
{
    /// <summary>
    /// Configures the native .NET 10 OpenAPI generation with enterprise-grade schema enhancements.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
    /// <returns>The <see cref="IServiceCollection"/> for further service registration.</returns>
    /// <remarks>
    /// This context standardizes the API documentation by applying a custom schema transformer
    /// (<see cref="ProblemDetailsSchemaTransformer"/>), specifically targeting RFC 7807 (Problem Details)
    /// to ensure consistency across microservices.
    /// <para>Features:</para>
    /// <list type="bullet">
    /// <item><description><b>ProblemDetails Enrichment:</b> Adds fields like <c>errors</c>, <c>requestId</c>, <c>traceId</c> and <c>nodeId</c> to the OpenAPI schema.</description></item>
    /// <item><description><b>DDD Integration:</b> Injects domain-specific properties (<c>aggregateId</c>, <c>aggregateCode</c>, <c>aggregateType</c>) into error responses for better debugging.</description></item>
    /// <item><description><b>Nullability Support:</b> Correctly renders <c>JsonSchemaType.Null</c> for optional properties in the documentation.</description></item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddOpenApiContext(this IServiceCollection services)
    {
        services.AddOpenApi(options => options.AddSchemaTransformer<ProblemDetailsSchemaTransformer>());

        return services;
    }
}
