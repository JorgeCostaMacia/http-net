using global::Serilog;
using global::Serilog.Core;
using global::Serilog.Events;
using JorgeCostaMacia.Exception.Domain;
using JorgeCostaMacia.Http.Exception.Serilog.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JorgeCostaMacia.Http.Exception.Serilog.Tests.Infrastructure;

// a concrete DomainException so the handler takes its Warning + aggregate-enrichment path.
file sealed class TestDomainException() : DomainException(null, null, null, null, null, "boom", null);

/// <summary>
/// End-to-end tests over <see cref="ExceptionHandler"/> — registered from the host as
/// <c>AddExceptionHandler&lt;ExceptionHandler&gt;()</c> — through the real ASP.NET Core exception
/// pipeline into a live Serilog sink. The unit tests assert only the log LEVEL
/// against an <see cref="ILogger"/> fake; only here do the fixed message ("Request Fail"/"Request Crash")
/// and the <c>LogContext</c> enrichment (aggregate metadata + user name) actually reach an emitted event.
/// </summary>
public class ExceptionHandlerIntegrationTests
{
    private sealed class CapturingSink : ILogEventSink
    {
        private readonly object _gate = new object();
        private readonly List<LogEvent> _events = new List<LogEvent>();

        public void Emit(LogEvent logEvent)
        {
            lock (_gate)
            {
                _events.Add(logEvent);
            }
        }

        public LogEvent Last(string message)
        {
            lock (_gate)
            {
                return _events.Last(logEvent => logEvent.MessageTemplate.Text == message);
            }
        }
    }

    private readonly CapturingSink _sink = new CapturingSink();

    private async Task<(WebApplication App, HttpClient Client)> App()
    {
        Log.Logger = new LoggerConfiguration().Enrich.FromLogContext().WriteTo.Sink(_sink).CreateLogger();

        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddSerilog();
        builder.Services.AddExceptionHandler<ExceptionHandler>();
        builder.Services.AddProblemDetails();   // gives UseExceptionHandler a response to write after the handler logs

        WebApplication app = builder.Build();
        app.UseExceptionHandler();
        app.MapGet("/domain", void () => throw new TestDomainException());
        app.MapGet("/boom", void () => throw new InvalidOperationException("crash"));

        await app.StartAsync(TestContext.Current.CancellationToken);

        return (app, app.GetTestClient());
    }

    [Fact]
    public async Task DomainException_LogsRequestFail_AtWarning_EnrichedWithAggregateMetadataAndUser()
    {
        (WebApplication app, HttpClient client) = await App();
        await using (app)
        {
            await client.GetAsync("/domain", TestContext.Current.CancellationToken);

            LogEvent logEvent = _sink.Last("Request Fail");
            Assert.Equal(LogEventLevel.Warning, logEvent.Level);
            Assert.True(logEvent.Properties.ContainsKey("ExceptionAggregateId"));
            Assert.True(logEvent.Properties.ContainsKey("ExceptionAggregateCode"));
            Assert.True(logEvent.Properties.ContainsKey("ExceptionAggregateType"));
            Assert.Equal("\"anonymous\"", logEvent.Properties["UserName"].ToString());
        }
    }

    [Fact]
    public async Task UnexpectedException_LogsRequestCrash_AtError()
    {
        (WebApplication app, HttpClient client) = await App();
        await using (app)
        {
            await client.GetAsync("/boom", TestContext.Current.CancellationToken);

            Assert.Equal(LogEventLevel.Error, _sink.Last("Request Crash").Level);
        }
    }
}
