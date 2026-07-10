using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JorgeCostaMacia.Http.OpenApi.Tests;

public class OpenApiContextTests
{
    private static async Task<JsonElement> ProblemDetailsSchema()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddOpenApiContext();

        await using WebApplication app = builder.Build();
        app.MapOpenApi();
        app.MapGet("/resource", () => "ok").ProducesProblem(StatusCodes.Status400BadRequest);

        await app.StartAsync(TestContext.Current.CancellationToken);

        string document = await app.GetTestClient().GetStringAsync("/openapi/v1.json", TestContext.Current.CancellationToken);

        return JsonDocument.Parse(document).RootElement
            .GetProperty("components").GetProperty("schemas").GetProperty("ProblemDetails").GetProperty("properties");
    }

    [Fact]
    public async Task Schema_DeclaresTheEnrichmentProperties()
    {
        JsonElement properties = await ProblemDetailsSchema();

        Assert.True(properties.TryGetProperty("errors", out _));
        Assert.True(properties.TryGetProperty("requestId", out _));
        Assert.True(properties.TryGetProperty("traceId", out _));
        Assert.True(properties.TryGetProperty("nodeId", out _));
        Assert.True(properties.TryGetProperty("aggregateId", out _));
        Assert.True(properties.TryGetProperty("aggregateCode", out _));
        Assert.True(properties.TryGetProperty("aggregateType", out _));
    }

    [Fact]
    public async Task Errors_IsADictionaryOfMessageArrays_NotNullOnly()
    {
        JsonElement errors = (await ProblemDetailsSchema()).GetProperty("errors");
        string raw = errors.GetRawText();

        Assert.Contains("object", raw);   // a generated client must type it as a real payload...
        Assert.Contains("null", raw);     // ...that may also be null for non-validation errors
        Assert.True(errors.TryGetProperty("additionalProperties", out JsonElement values));
        Assert.Contains("array", values.GetRawText());
    }
}
