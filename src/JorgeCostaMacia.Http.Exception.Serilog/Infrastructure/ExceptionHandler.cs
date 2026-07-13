using global::Serilog.Context;
using JorgeCostaMacia.Exception.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace JorgeCostaMacia.Http.Exception.Serilog.Infrastructure;

/// <summary>
/// Logs every unhandled exception that reaches the exception handling pipeline, enriched with
/// aggregate metadata (for a <see cref="DomainException"/>) and the authenticated user's name.
/// </summary>
/// <remarks>
/// Always returns <see langword="false"/>, since this handler only logs as a side effect and
/// never produces the response itself — that remains the responsibility of
/// <c>ExceptionContext</c>'s status code mapping, which runs after this handler in the
/// <see cref="IExceptionHandler"/> chain.
/// </remarks>
internal sealed class ExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ExceptionHandler> _logger;

    public ExceptionHandler(ILogger<ExceptionHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Logs the exception at <see cref="LogLevel.Warning"/> for known/expected exception types
    /// (<see cref="DomainException"/>, <see cref="BadHttpRequestException"/>, or
    /// <see cref="FluentValidation.ValidationException"/>), or at <see cref="LogLevel.Error"/>
    /// for any other (unexpected) exception.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> for the failed request.</param>
    /// <param name="exception">The unhandled exception being logged.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><see langword="false"/>, always.</returns>
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, System.Exception exception, CancellationToken cancellationToken)
    {
        DomainException? domainEx = exception as DomainException;

        using (LogContext.PushProperty("ExceptionAggregateId", domainEx?.AggregateId))
        using (LogContext.PushProperty("ExceptionAggregateCode", domainEx?.AggregateCode))
        using (LogContext.PushProperty("ExceptionAggregateType", domainEx?.AggregateType))
        using (LogContext.PushProperty("UserName", httpContext.User?.Identity?.Name ?? "anonymous"))
        {
            if (exception is DomainException or BadHttpRequestException or FluentValidation.ValidationException)
            {
                _logger.LogWarning(exception, "Request Fail");
            }
            else
            {
                _logger.LogError(exception, "Request Crash");
            }
        }

        return ValueTask.FromResult(false);
    }
}
