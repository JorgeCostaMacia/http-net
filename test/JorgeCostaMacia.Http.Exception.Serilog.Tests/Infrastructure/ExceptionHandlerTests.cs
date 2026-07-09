using FluentValidation.Results;
using JorgeCostaMacia.Exception.Domain;
using JorgeCostaMacia.Http.Exception.Serilog.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace JorgeCostaMacia.Http.Exception.Serilog.Tests.Infrastructure;

file sealed class TestDomainException(string? message = null)
    : DomainException(null, null, null, null, null, message, null);

public class ExceptionHandlerTests
{
    private sealed class LoggerFake : ILogger<ExceptionHandler>
    {
        public List<(LogLevel Level, System.Exception? Exception)> Logged { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, System.Exception? exception, Func<TState, System.Exception?, string> formatter)
            => Logged.Add((logLevel, exception));
    }

    private readonly LoggerFake _logger = new();

    private async Task<bool> Handle(System.Exception exception)
        => await new ExceptionHandler(_logger).TryHandleAsync(new DefaultHttpContext(), exception, TestContext.Current.CancellationToken);

    [Fact]
    public async Task DomainException_LogsWarning()
    {
        await Handle(new TestDomainException("boom"));

        Assert.Equal(LogLevel.Warning, Assert.Single(_logger.Logged).Level);
    }

    [Fact]
    public async Task BadHttpRequestException_LogsWarning()
    {
        await Handle(new BadHttpRequestException("bad"));

        Assert.Equal(LogLevel.Warning, Assert.Single(_logger.Logged).Level);
    }

    [Fact]
    public async Task FluentValidationException_LogsWarning()
    {
        await Handle(new FluentValidation.ValidationException(new List<ValidationFailure>()));

        Assert.Equal(LogLevel.Warning, Assert.Single(_logger.Logged).Level);
    }

    [Fact]
    public async Task UnexpectedException_LogsError()
    {
        await Handle(new InvalidOperationException("crash"));

        Assert.Equal(LogLevel.Error, Assert.Single(_logger.Logged).Level);
    }

    [Fact]
    public async Task AlwaysReturnsFalse_SoThePipelineStillProducesTheResponse()
        => Assert.False(await Handle(new TestDomainException("boom")));
}
