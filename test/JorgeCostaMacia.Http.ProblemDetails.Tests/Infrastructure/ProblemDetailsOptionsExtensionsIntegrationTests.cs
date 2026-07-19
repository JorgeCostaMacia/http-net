using System.Net;
using System.Text;
using System.Text.Json;
using FluentValidation.Results;
using JorgeCostaMacia.Exception.Domain;
using JorgeCostaMacia.Http.ProblemDetails.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JorgeCostaMacia.Http.ProblemDetails.Tests.Infrastructure;

// concrete subclasses: the family exceptions carry protected ctors (one per aggregate in real code).
file sealed class TestDomainException() : DomainException(null, null, null, null, null, "boom", null);

// a compound property name ("FirstName") is the discriminator: only a real ConvertName pass turns its
// key into "first_name" and rewrites the quoted name inside the message.
file sealed class TestValidationException()
    : ValidationException(null, null, null, null, null, null, null, new List<ValidationFailure> { new ValidationFailure("FirstName", "'FirstName' is invalid") });

file sealed record Payload(int Value);

/// <summary>
/// End-to-end tests over <see cref="ProblemDetailsOptionsExtensions.WithDefaults"/> through the real
/// ASP.NET Core exception pipeline (<c>UseExceptionHandler</c> → <c>IProblemDetailsService</c> → the
/// configured <see cref="JsonSerializerOptions"/>). This is the only place the emitted RFC 7807 body is
/// provable: the unit tests call the handlers directly and never exercise the middleware, the
/// <c>RequestId</c>/<c>TraceId</c>/<c>NodeId</c> enrichment, or JSON-key casing under a live policy.
/// The HTTP status itself is the exception-handler's contract, asserted in Http.Exception.Tests — here
/// every unhandled exception surfaces as the pipeline default, so only the body is under test.
/// </summary>
public class ProblemDetailsOptionsExtensionsIntegrationTests
{
    private static async Task<(WebApplication App, HttpClient Client)> App()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        // mirror the real host's response-serialization config: a non-default naming policy the package
        // must honor on every JSON key. (The app also sets deserialization options — case-insensitivity,
        // trailing commas, required-ctor params — which don't affect the error response written here.)
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
            options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
        });
        builder.Services.AddProblemDetails(options => options.WithDefaults());
        // minimal-API binding failures return a bare 400 unless this is set — only then do they throw a
        // BadHttpRequestException that reaches the pipeline and gets a ProblemDetails body.
        builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

        WebApplication app = builder.Build();
        app.UseExceptionHandler();
        app.MapGet("/domain", void () => throw new TestDomainException());
        app.MapGet("/domain-validation", void () => throw new TestValidationException());
        app.MapGet("/fluent-validation", void () => throw new FluentValidation.ValidationException(new List<ValidationFailure> { new ValidationFailure("FirstName", "'FirstName' is invalid") }));
        app.MapGet("/boom", void () => throw new InvalidOperationException("crash"));
        app.MapPost("/typed", (Payload payload) => Results.Ok(payload));

        await app.StartAsync(TestContext.Current.CancellationToken);

        return (app, app.GetTestClient());
    }

    private static async Task<JsonElement> Body(HttpResponseMessage response)
    {
        string json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        return JsonDocument.Parse(json).RootElement;
    }

    [Fact]
    public async Task Response_ContentType_IsApplicationProblemJson()
    {
        (WebApplication app, HttpClient client) = await App();
        await using (app)
        {
            HttpResponseMessage response = await client.GetAsync("/domain", TestContext.Current.CancellationToken);

            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        }
    }

    [Fact]
    public async Task DomainException_Body_UsesConvertedKeys_AndRawTypeValue()
    {
        (WebApplication app, HttpClient client) = await App();
        await using (app)
        {
            HttpResponseMessage response = await client.GetAsync("/domain", TestContext.Current.CancellationToken);

            JsonElement body = await Body(response);
            // keys travel through the configured policy...
            Assert.True(body.TryGetProperty("aggregate_id", out JsonElement _));
            Assert.True(body.TryGetProperty("aggregate_code", out JsonElement _));
            // ...but the type is a data VALUE and stays raw, so client + log correlation see one spelling.
            Assert.Equal("DomainException", body.GetProperty("aggregate_type").GetString());
            Assert.Equal("GET /domain", body.GetProperty("instance").GetString());
        }
    }

    [Fact]
    public async Task EveryErrorResponse_CarriesRequestIdTraceIdAndNodeId()
    {
        (WebApplication app, HttpClient client) = await App();
        await using (app)
        {
            HttpResponseMessage response = await client.GetAsync("/domain", TestContext.Current.CancellationToken);

            JsonElement body = await Body(response);
            Assert.True(body.TryGetProperty("request_id", out JsonElement _));
            Assert.True(body.TryGetProperty("trace_id", out JsonElement _));
            Assert.False(string.IsNullOrEmpty(body.GetProperty("node_id").GetString()));
        }
    }

    [Fact]
    public async Task DomainValidationException_Body_GroupsErrorsByConvertedFieldName()
    {
        (WebApplication app, HttpClient client) = await App();
        await using (app)
        {
            HttpResponseMessage response = await client.GetAsync("/domain-validation", TestContext.Current.CancellationToken);

            JsonElement errors = (await Body(response)).GetProperty("errors");
            Assert.Equal("'first_name' is invalid", errors.GetProperty("first_name")[0].GetString());
        }
    }

    [Fact]
    public async Task RawFluentValidationException_Body_GroupsErrorsByConvertedFieldName()
    {
        (WebApplication app, HttpClient client) = await App();
        await using (app)
        {
            HttpResponseMessage response = await client.GetAsync("/fluent-validation", TestContext.Current.CancellationToken);

            JsonElement errors = (await Body(response)).GetProperty("errors");
            Assert.Equal("'first_name' is invalid", errors.GetProperty("first_name")[0].GetString());
        }
    }

    [Fact]
    public async Task UnexpectedException_Body_NullsAggregateMetadataAndErrors()
    {
        (WebApplication app, HttpClient client) = await App();
        await using (app)
        {
            HttpResponseMessage response = await client.GetAsync("/boom", TestContext.Current.CancellationToken);

            JsonElement body = await Body(response);
            Assert.Equal(JsonValueKind.Null, body.GetProperty("aggregate_id").ValueKind);
            Assert.Equal(JsonValueKind.Null, body.GetProperty("errors").ValueKind);
        }
    }

    [Fact]
    public async Task BadHttpRequest_InvalidFieldType_Body_GroupsErrorByConvertedFieldName()
    {
        (WebApplication app, HttpClient client) = await App();
        await using (app)
        {
            HttpResponseMessage response = await client.PostAsync("/typed", new StringContent("{\"value\":\"x\"}", Encoding.UTF8, "application/json"), TestContext.Current.CancellationToken);

            JsonElement errors = (await Body(response)).GetProperty("errors");
            Assert.Equal("'value' has an invalid data type format.", errors.GetProperty("value")[0].GetString());
        }
    }

    [Fact]
    public async Task BadHttpRequest_EmptyBody_Body_ReportsMissingBody()
    {
        // .NET 10's empty-body wording ("Implicit body inferred ... but no body was provided.") differs
        // from older runtimes' ("Required parameter ... was not provided from body."); the handler must
        // match both so an empty body reports the missing-body error instead of degrading to null.
        (WebApplication app, HttpClient client) = await App();
        await using (app)
        {
            HttpResponseMessage response = await client.PostAsync("/typed", new StringContent("", Encoding.UTF8, "application/json"), TestContext.Current.CancellationToken);

            JsonElement errors = (await Body(response)).GetProperty("errors");
            Assert.Equal("A non-empty request body is required.", errors.GetProperty("request")[0].GetString());
        }
    }
}
