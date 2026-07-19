using System.Net;
using FluentValidation.Results;
using JorgeCostaMacia.Exception.Domain;
using JorgeCostaMacia.Http.Exception.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JorgeCostaMacia.Http.Exception.Tests;

file sealed class TestNotFoundException() : NotFoundException(null, null, null, null, null, "missing", null);

file sealed class TestExistException() : ExistException(null, null, null, null, null, "duplicate", null);

file sealed class TestValidationException() : ValidationException(null, null, null, null, null, null, null, new List<ValidationFailure>());

public class ExceptionHandlerOptionsExtensionsTests
{
    private static async Task<HttpResponseMessage> Request(System.Exception exception)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddProblemDetails();

        await using WebApplication app = builder.Build();
        app.UseExceptionHandler(new ExceptionHandlerOptions().WithDefaultStatusCodes());
        app.MapGet("/throw", () => { throw exception; });

        await app.StartAsync(TestContext.Current.CancellationToken);

        return await app.GetTestClient().GetAsync("/throw", TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task NotFoundException_MapsTo404()
        => Assert.Equal(HttpStatusCode.NotFound, (await Request(new TestNotFoundException())).StatusCode);

    [Fact]
    public async Task ExistException_MapsTo409()
        => Assert.Equal(HttpStatusCode.Conflict, (await Request(new TestExistException())).StatusCode);

    [Fact]
    public async Task DomainValidationException_MapsTo400()
        => Assert.Equal(HttpStatusCode.BadRequest, (await Request(new TestValidationException())).StatusCode);

    [Fact]
    public async Task FluentValidationException_MapsTo400()
        => Assert.Equal(HttpStatusCode.BadRequest, (await Request(new FluentValidation.ValidationException(new List<ValidationFailure>()))).StatusCode);

    [Fact]
    public async Task BadHttpRequestException_UsesItsOwnStatusCode()
        => Assert.Equal(HttpStatusCode.UnsupportedMediaType, (await Request(new BadHttpRequestException("bad", StatusCodes.Status415UnsupportedMediaType))).StatusCode);

    [Fact]
    public async Task UnauthorizedAccessException_MapsTo403()
        => Assert.Equal(HttpStatusCode.Forbidden, (await Request(new UnauthorizedAccessException())).StatusCode);

    [Fact]
    public async Task NotImplementedException_MapsTo501()
        => Assert.Equal(HttpStatusCode.NotImplemented, (await Request(new NotImplementedException())).StatusCode);

    [Fact]
    public async Task OperationCanceledException_MapsTo499()
        => Assert.Equal((HttpStatusCode)499, (await Request(new OperationCanceledException())).StatusCode);

    [Fact]
    public async Task TaskCanceledException_MapsTo499_ViaItsOperationCanceledBase()
        => Assert.Equal((HttpStatusCode)499, (await Request(new TaskCanceledException())).StatusCode);

    [Fact]
    public async Task UnexpectedException_MapsTo500()
        => Assert.Equal(HttpStatusCode.InternalServerError, (await Request(new InvalidOperationException("crash"))).StatusCode);
}
