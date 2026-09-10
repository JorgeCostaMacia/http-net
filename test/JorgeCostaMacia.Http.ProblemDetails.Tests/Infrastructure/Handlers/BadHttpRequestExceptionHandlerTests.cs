using System.Text.Json;
using JorgeCostaMacia.Http.ProblemDetails.Infrastructure.Handlers;
using Microsoft.AspNetCore.Http;

namespace JorgeCostaMacia.Http.ProblemDetails.Tests.Infrastructure.Handlers;

public class BadHttpRequestExceptionHandlerTests
{
    private static Microsoft.AspNetCore.Http.ProblemDetailsContext Context()
        => new Microsoft.AspNetCore.Http.ProblemDetailsContext() { HttpContext = new DefaultHttpContext() };

    [Fact]
    public void Handle_NullsTheAggregateExtensions()
    {
        Microsoft.AspNetCore.Http.ProblemDetailsContext ctx = Context();

        BadHttpRequestExceptionHandler.Handle(ctx, new BadHttpRequestException("whatever"), JsonNamingPolicy.CamelCase);

        Assert.Null(ctx.ProblemDetails.Extensions["aggregateId"]);
        Assert.Null(ctx.ProblemDetails.Extensions["aggregateCode"]);
        Assert.Null(ctx.ProblemDetails.Extensions["aggregateType"]);
    }

    [Fact]
    public void Handle_UnrecognizedWording_KeepsErrorsPresentAndNull()
    {
        // a runtime rewording (or another version's message) must not silently drop the field
        Microsoft.AspNetCore.Http.ProblemDetailsContext ctx = Context();

        BadHttpRequestExceptionHandler.Handle(ctx, new BadHttpRequestException("some future wording"), JsonNamingPolicy.CamelCase);

        Assert.True(ctx.ProblemDetails.Extensions.ContainsKey("errors"));
        Assert.Null(ctx.ProblemDetails.Extensions["errors"]);
    }

    [Fact]
    public void Handle_MissingBodyWording_MapsToRequestError()
    {
        Microsoft.AspNetCore.Http.ProblemDetailsContext ctx = Context();

        BadHttpRequestExceptionHandler.Handle(ctx, new BadHttpRequestException("Required parameter \"TestRequest request\" was not provided from body."), JsonNamingPolicy.CamelCase);

        Dictionary<string, string[]> errors = Assert.IsType<Dictionary<string, string[]>>(ctx.ProblemDetails.Extensions["errors"]);
        Assert.Equal("A non-empty request body is required.", Assert.Single(errors["request"]));
    }

    [Fact]
    public void Handle_MissingBodyWording_Net10_MapsToRequestError()
    {
        // .NET 10 reworded the empty-body case to "Implicit body inferred ... but no body was provided."
        Microsoft.AspNetCore.Http.ProblemDetailsContext ctx = Context();

        BadHttpRequestExceptionHandler.Handle(ctx, new BadHttpRequestException("Implicit body inferred for parameter \"request\" but no body was provided. Did you mean to use a Service instead?"), JsonNamingPolicy.CamelCase);

        Dictionary<string, string[]> errors = Assert.IsType<Dictionary<string, string[]>>(ctx.ProblemDetails.Extensions["errors"]);
        Assert.Equal("A non-empty request body is required.", Assert.Single(errors["request"]));
    }

    [Fact]
    public void Handle_UnreadableJsonWording_MapsToRequestError()
    {
        Microsoft.AspNetCore.Http.ProblemDetailsContext ctx = Context();

        BadHttpRequestExceptionHandler.Handle(ctx, new BadHttpRequestException("Failed to read parameter \"TestRequest request\" from the request body as JSON."), JsonNamingPolicy.CamelCase);

        Dictionary<string, string[]> errors = Assert.IsType<Dictionary<string, string[]>>(ctx.ProblemDetails.Extensions["errors"]);
        Assert.Equal("One or more fields have an invalid data type format.", Assert.Single(errors["request"]));
    }
    // The two JsonException paths. A real request on net10 lands in the wording branches instead, so
    // nothing was reaching these — they are the shape the runtime raises when the body deserializes
    // into a type with required members, and they have to keep working if that shape comes back.
    [Fact]
    public void Handle_MissingRequiredProperties_MapsEachFieldToItsOwnError()
    {
        Microsoft.AspNetCore.Http.ProblemDetailsContext ctx = Context();
        // The runtime's own wording, verified against System.Text.Json on net10: the type is quoted too.
        JsonException inner = new JsonException("JSON deserialization for type 'Sample' was missing required properties including: 'firstName'; 'lastName'.");

        BadHttpRequestExceptionHandler.Handle(ctx, new BadHttpRequestException("bad", inner), JsonNamingPolicy.CamelCase);

        Dictionary<string, string[]> errors = Assert.IsType<Dictionary<string, string[]>>(ctx.ProblemDetails.Extensions["errors"]);
        Assert.Equal(2, errors.Count);
        Assert.Equal("'firstName' must be present.", Assert.Single(errors["firstName"]));
        Assert.Equal("'lastName' must be present.", Assert.Single(errors["lastName"]));
    }

    // The field names are quoted in the message; when none can be pulled out, the report falls back to
    // one generic entry rather than an empty errors object.
    [Fact]
    public void Handle_MissingRequiredProperties_WithNoExtractableField_FallsBackToOneRequestError()
    {
        Microsoft.AspNetCore.Http.ProblemDetailsContext ctx = Context();
        JsonException inner = new JsonException("JSON deserialization for type 'Sample' was missing required properties including: 'request'; 'nested.field'.");

        BadHttpRequestExceptionHandler.Handle(ctx, new BadHttpRequestException("bad", inner), JsonNamingPolicy.CamelCase);

        Dictionary<string, string[]> errors = Assert.IsType<Dictionary<string, string[]>>(ctx.ProblemDetails.Extensions["errors"]);
        Assert.Equal("One or more required fields are not present.", Assert.Single(errors["request"]));
    }

    [Fact]
    public void Handle_InvalidPropertyType_NamesTheFieldFromTheJsonPath()
    {
        Microsoft.AspNetCore.Http.ProblemDetailsContext ctx = Context();
        // A real path from the runtime: $.age for a field that failed to convert.
        JsonException inner = new JsonException("The JSON value could not be converted to System.Int32.", "$.age", 0, 56);

        BadHttpRequestExceptionHandler.Handle(ctx, new BadHttpRequestException("bad", inner), JsonNamingPolicy.CamelCase);

        Dictionary<string, string[]> errors = Assert.IsType<Dictionary<string, string[]>>(ctx.ProblemDetails.Extensions["errors"]);
        Assert.Equal("'age' has an invalid data type format.", Assert.Single(errors["age"]));
    }

    // A path with nothing after the root ("$.") names no field, so the report stays generic.
    [Fact]
    public void Handle_InvalidPropertyType_WithNoFieldInThePath_FallsBackToOneRequestError()
    {
        Microsoft.AspNetCore.Http.ProblemDetailsContext ctx = Context();
        JsonException inner = new JsonException("The JSON value could not be converted.", "$.", 0, 0);

        BadHttpRequestExceptionHandler.Handle(ctx, new BadHttpRequestException("bad", inner), JsonNamingPolicy.CamelCase);

        Dictionary<string, string[]> errors = Assert.IsType<Dictionary<string, string[]>>(ctx.ProblemDetails.Extensions["errors"]);
        Assert.Equal("One or more fields have an invalid data type format.", Assert.Single(errors["request"]));
    }
}
