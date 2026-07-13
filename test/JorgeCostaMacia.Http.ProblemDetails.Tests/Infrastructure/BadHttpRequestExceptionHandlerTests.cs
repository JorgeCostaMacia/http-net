using System.Text.Json;
using JorgeCostaMacia.Http.ProblemDetails.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace JorgeCostaMacia.Http.ProblemDetails.Tests.Infrastructure;

public class BadHttpRequestExceptionHandlerTests
{
    private static Microsoft.AspNetCore.Http.ProblemDetailsContext Context()
        => new() { HttpContext = new DefaultHttpContext() };

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
}
