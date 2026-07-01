using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

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
    /// This context standardizes the API documentation by applying custom schema transformers,
    /// specifically targeting RFC 7807 (Problem Details) to ensure consistency across microservices.
    /// <para>Features:</para>
    /// <list type="bullet">
    /// <item><description><b>ProblemDetails Enrichment:</b> Adds fields like <c>errors</c>, <c>requestId</c>, <c>traceId</c> and <c>nodeId</c> to the OpenAPI schema.</description></item>
    /// <item><description><b>DDD Integration:</b> Injects domain-specific properties (<c>aggregateId</c>, <c>aggregateCode</c>, <c>aggregateType</c>) into error responses for better debugging.</description></item>
    /// <item><description><b>Native Performance:</b> Uses the built-in Microsoft OpenAPI engine, avoiding heavy third-party dependencies.</description></item>
    /// <item><description><b>Nullability Support:</b> Correctly renders <c>JsonSchemaType.Null</c> for optional properties in the documentation.</description></item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddOpenApiContext(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddSchemaTransformer((schema, context, cancellationToken) =>
            {
                if (context.JsonTypeInfo.Type == typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))
                {
                    schema.Properties?.TryAdd("errors", new OpenApiSchema { Type = JsonSchemaType.Null });
                }

                if (context.JsonTypeInfo.Type == typeof(Microsoft.AspNetCore.Mvc.ProblemDetails) || context.JsonTypeInfo.Type == typeof(Microsoft.AspNetCore.Mvc.ValidationProblemDetails) || context.JsonTypeInfo.Type == typeof(Microsoft.AspNetCore.Http.HttpValidationProblemDetails))
                {
                    schema.Properties?.TryAdd("requestId", new OpenApiSchema { Type = JsonSchemaType.String });
                    schema.Properties?.TryAdd("traceId", new OpenApiSchema { Type = JsonSchemaType.String });
                    schema.Properties?.TryAdd("nodeId", new OpenApiSchema { Type = JsonSchemaType.String });

                    schema.Properties?.TryAdd("aggregateId", new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null, Format = "uuid" });
                    schema.Properties?.TryAdd("aggregateCode", new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null, Format = "uuid" });
                    schema.Properties?.TryAdd("aggregateType", new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null });
                }

                return Task.CompletedTask;
            });
        });

        return services;
    }
}
