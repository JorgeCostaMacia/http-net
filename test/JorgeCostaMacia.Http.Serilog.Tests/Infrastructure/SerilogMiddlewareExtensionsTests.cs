using System.Net;
using System.Text;
using global::Serilog;
using global::Serilog.Core;
using global::Serilog.Events;
using JorgeCostaMacia.Http.Serilog.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// 'Serilog' (the third-party namespace) is referenced with global:: so it isn't shadowed by the
// enclosing 'JorgeCostaMacia.Http.Serilog' package namespace in test code.
namespace JorgeCostaMacia.Http.Serilog.Tests.Infrastructure;

public class SerilogMiddlewareExtensionsTests
{
    /// <summary>A trivial capture sink — no AsyncLocal magic, every emitted event lands here.</summary>
    private sealed class CaptureSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = new List<LogEvent>();

        public void Emit(LogEvent logEvent) => Events.Add(logEvent);
    }

    private readonly CaptureSink _sink = new CaptureSink();

    private async Task<(WebApplication App, HttpClient Client)> App()
    {
        Log.Logger = new LoggerConfiguration().Enrich.FromLogContext().WriteTo.Sink(_sink).CreateLogger();

        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddSerilog();   // registers the static logger above (the middleware's fallback too)

        WebApplication app = builder.Build();
        app.UseSerilogBodyBuffer();
        app.UseSerilogEnrichRequest();
        app.UseSerilogEnrichAuthentication();
        app.UseSerilogRequestSummary();
        app.MapPost("/echo", async (HttpRequest request) =>
        {
            using StreamReader reader = new StreamReader(request.Body);
            return Results.Text(await reader.ReadToEndAsync());
        });
        app.MapGet("/ok", () => Results.Ok());
        app.MapGet("/missing", () => Results.NotFound());

        await app.StartAsync(TestContext.Current.CancellationToken);

        return (app, app.GetTestClient());
    }

    private LogEvent Completion()
        => _sink.Events.Last(logEvent => logEvent.MessageTemplate.Text.Contains("Request End"));

    [Fact]
    public async Task Body_IsCaptured_AndTheEndpointCanStillReadIt()
    {
        (WebApplication app, HttpClient client) = await App();
        await using (app)
        {
            HttpResponseMessage response = await client.PostAsync("/echo", new StringContent("{\"name\":\"pepe\"}", Encoding.UTF8, "application/json"), TestContext.Current.CancellationToken);

            // the endpoint read the body AFTER the enrichment already consumed it — buffering works
            Assert.Equal("{\"name\":\"pepe\"}", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

            LogEvent completion = Completion();
            Assert.Equal("{\"name\":\"pepe\"}", ((ScalarValue)completion.Properties["RequestBody"]).Value);
            Assert.Equal("anonymous", ((ScalarValue)completion.Properties["UserName"]).Value);
        }
    }

    [Fact]
    public async Task Completion_LogsInformation_OnSuccess()
    {
        (WebApplication app, HttpClient client) = await App();
        await using (app)
        {
            await client.GetAsync("/ok", TestContext.Current.CancellationToken);

            Assert.Equal(LogEventLevel.Information, Completion().Level);
        }
    }

    [Fact]
    public async Task Completion_LogsWarning_OnClientError()
    {
        (WebApplication app, HttpClient client) = await App();
        await using (app)
        {
            HttpResponseMessage response = await client.GetAsync("/missing", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(LogEventLevel.Warning, Completion().Level);
        }
    }
}
