using JorgeCostaMacia.Http.Serilog.Infrastructure;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.AspNetCore;
using Serilog.Events;

namespace JorgeCostaMacia.Http.Serilog.Tests.Infrastructure;

/// <summary>
/// The request-logging policy: which level a completed request is logged at, and what the diagnostic
/// context carries. Both are delegates on <see cref="RequestLoggingOptions"/>, so they are invoked here
/// directly — the level mapping is a decision worth pinning per outcome rather than sampling through a
/// live pipeline.
/// </summary>
public class RequestLoggingOptionsExtensionsTests
{
    private static RequestLoggingOptions Configured() => new RequestLoggingOptions().WithDefaults();

    private static HttpContext ContextWith(int statusCode)
    {
        DefaultHttpContext context = new DefaultHttpContext();
        context.Response.StatusCode = statusCode;

        return context;
    }

    [Fact]
    public void WithDefaults_ReturnsTheSameOptions_SoItChains()
    {
        RequestLoggingOptions options = new RequestLoggingOptions();

        Assert.Same(options, options.WithDefaults());
    }

    [Fact]
    public void WithDefaults_SetsTheFixedMessageTemplate()
        => Assert.Equal("Request End", Configured().MessageTemplate);

    // An exception outranks the status code: a request that threw is an error even if the response was
    // already written with a 2xx.
    [Fact]
    public void GetLevel_WithAnException_IsError()
        => Assert.Equal(LogEventLevel.Error, Configured().GetLevel(ContextWith(200), 0, new InvalidOperationException("boom")));

    [Theory]
    [InlineData(500)]
    [InlineData(503)]
    public void GetLevel_OnServerError_IsError(int statusCode)
        => Assert.Equal(LogEventLevel.Error, Configured().GetLevel(ContextWith(statusCode), 0, null));

    [Theory]
    [InlineData(400)]
    [InlineData(404)]
    [InlineData(499)]
    public void GetLevel_OnClientError_IsWarning(int statusCode)
        => Assert.Equal(LogEventLevel.Warning, Configured().GetLevel(ContextWith(statusCode), 0, null));

    [Theory]
    [InlineData(200)]
    [InlineData(302)]
    [InlineData(399)]
    public void GetLevel_OnSuccessOrRedirect_IsInformation(int statusCode)
        => Assert.Equal(LogEventLevel.Information, Configured().GetLevel(ContextWith(statusCode), 0, null));

    // The boundaries the comparisons actually use: 399 is still Information and 499 still Warning, so a
    // ">=" slipped in for a ">" would show up here.
    [Fact]
    public void GetLevel_AtTheBoundaries_KeepsTheLowerLevel()
    {
        RequestLoggingOptions options = Configured();

        Assert.Equal(LogEventLevel.Information, options.GetLevel(ContextWith(399), 0, null));
        Assert.Equal(LogEventLevel.Warning, options.GetLevel(ContextWith(400), 0, null));
        Assert.Equal(LogEventLevel.Warning, options.GetLevel(ContextWith(499), 0, null));
        Assert.Equal(LogEventLevel.Error, options.GetLevel(ContextWith(500), 0, null));
    }

    [Fact]
    public void EnrichDiagnosticContext_WithNoAuthenticatedUser_SetsAnonymous()
    {
        DiagnosticContextFake diagnostics = new DiagnosticContextFake();

        Configured().EnrichDiagnosticContext!(diagnostics, new DefaultHttpContext());

        Assert.Equal("anonymous", diagnostics.Values["UserName"]);
    }

    [Fact]
    public void EnrichDiagnosticContext_WithAnAuthenticatedUser_SetsTheName()
    {
        DiagnosticContextFake diagnostics = new DiagnosticContextFake();
        DefaultHttpContext context = new DefaultHttpContext();
        context.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(
                new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "jcosta") },
                "test"));

        Configured().EnrichDiagnosticContext!(diagnostics, context);

        Assert.Equal("jcosta", diagnostics.Values["UserName"]);
    }

    /// <summary>Diagnostic-context double: a bag of what was set, which is all the policy needs.</summary>
    private sealed class DiagnosticContextFake : IDiagnosticContext
    {
        public Dictionary<string, object?> Values { get; } = new Dictionary<string, object?>();

        public void Set(string propertyName, object? value, bool destructureObjects = false)
            => Values[propertyName] = value;

        public void SetException(System.Exception exception) => Values["Exception"] = exception;
    }
}
