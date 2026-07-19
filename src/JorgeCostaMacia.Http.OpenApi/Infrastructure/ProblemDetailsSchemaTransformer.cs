using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace JorgeCostaMacia.Http.OpenApi.Infrastructure;

/// <summary>
/// Enriches the generated OpenAPI schema for RFC 7807 Problem Details with the tracing and DDD fields
/// the API actually returns on failures — <c>errors</c>, <c>requestId</c>, <c>traceId</c>, <c>nodeId</c>,
/// and the aggregate context (<c>aggregateId</c>, <c>aggregateCode</c>, <c>aggregateType</c>) — so
/// generated clients see the real payload shape instead of a bare <c>ProblemDetails</c>.
/// </summary>
public sealed class ProblemDetailsSchemaTransformer : IOpenApiSchemaTransformer
{
    /// <summary>Adds the enrichment properties to the ProblemDetails / ValidationProblemDetails schemas.</summary>
    /// <param name="schema">The schema being transformed.</param>
    /// <param name="context">The transformer context, carrying the CLR type the schema maps.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.JsonTypeInfo.Type == typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))
        {
            // a validation failure fills it with { property: [messages] }; null for the rest —
            // a null-only type would make generated clients drop the payload exactly when it matters.
            schema.Properties?.TryAdd("errors", new OpenApiSchema
            {
                Type = JsonSchemaType.Object | JsonSchemaType.Null,
                AdditionalProperties = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = new OpenApiSchema { Type = JsonSchemaType.String }
                }
            });
        }

        if (context.JsonTypeInfo.Type == typeof(Microsoft.AspNetCore.Mvc.ProblemDetails) || context.JsonTypeInfo.Type == typeof(Microsoft.AspNetCore.Mvc.ValidationProblemDetails) || context.JsonTypeInfo.Type == typeof(Microsoft.AspNetCore.Http.HttpValidationProblemDetails))
        {
            schema.Properties?.TryAdd("requestId", new OpenApiSchema { Type = JsonSchemaType.String });
            schema.Properties?.TryAdd("traceId", new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null });
            schema.Properties?.TryAdd("nodeId", new OpenApiSchema { Type = JsonSchemaType.String });

            schema.Properties?.TryAdd("aggregateId", new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null, Format = "uuid" });
            schema.Properties?.TryAdd("aggregateCode", new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null, Format = "uuid" });
            schema.Properties?.TryAdd("aggregateType", new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null });
        }

        return Task.CompletedTask;
    }
}
