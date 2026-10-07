using System.Net;
using JorgeCostaMacia.Http.ForwardedHeaders.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace JorgeCostaMacia.Http.ForwardedHeaders.Tests.Infrastructure;

/// <summary>
/// End-to-end tests over <see cref="ForwardedHeadersOptionsExtensions.WithDefaults"/> through the real
/// <c>UseForwardedHeaders</c> middleware: what a request ends up saying about its client depends on who sent the
/// headers, and only the middleware can show that.
/// </summary>
public class ForwardedHeadersOptionsExtensionsIntegrationTests
{
    private static async Task<HttpContext> Send(IPAddress from)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.Configure<ForwardedHeadersOptions>(options => options.WithDefaults());

        await using WebApplication app = builder.Build();
        app.UseForwardedHeaders();
        app.Run(_ => Task.CompletedTask);
        await app.StartAsync(TestContext.Current.CancellationToken);

        return await app.GetTestServer().SendAsync(context =>
        {
            context.Connection.RemoteIpAddress = from;
            context.Request.Scheme = "http";
            context.Request.Host = new HostString("container:8080");
            context.Request.Headers["X-Forwarded-For"] = "203.0.113.7";
            context.Request.Headers["X-Forwarded-Proto"] = "https";
            context.Request.Headers["X-Forwarded-Host"] = "api.example.com";
        }, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task FromDockersBridge_TheRequestTakesTheProxysClientSchemeAndHost()
    {
        HttpContext context = await Send(IPAddress.Parse("172.17.0.1"));

        Assert.Equal(IPAddress.Parse("203.0.113.7"), context.Connection.RemoteIpAddress);
        Assert.Equal("https", context.Request.Scheme);
        Assert.Equal("api.example.com", context.Request.Host.Value);
    }

    [Fact]
    public async Task FromTheLoopback_TheRequestTakesTheProxysClient()
        => Assert.Equal(IPAddress.Parse("203.0.113.7"), (await Send(IPAddress.Loopback)).Connection.RemoteIpAddress);

    // a client that sends the headers itself cannot forge its address or scheme
    [Fact]
    public async Task FromAnUntrustedAddress_TheHeadersAreIgnored()
    {
        HttpContext context = await Send(IPAddress.Parse("198.51.100.9"));

        Assert.Equal(IPAddress.Parse("198.51.100.9"), context.Connection.RemoteIpAddress);
        Assert.Equal("http", context.Request.Scheme);
        Assert.Equal("container:8080", context.Request.Host.Value);
    }
}
