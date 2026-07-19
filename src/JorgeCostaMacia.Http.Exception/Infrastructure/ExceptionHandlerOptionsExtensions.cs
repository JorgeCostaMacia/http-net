using JorgeCostaMacia.Exception.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace JorgeCostaMacia.Http.Exception.Infrastructure;

/// <summary>
/// Extensions for <see cref="ExceptionHandlerOptions"/> that apply the default HTTP status-code policy
/// to the exception-handling pipeline. Kept as an <see cref="ExceptionHandlerOptions"/> extension (not a
/// hidden <c>Use…</c> facade) so the <c>UseExceptionHandler</c> call stays visible in the host's
/// <c>Program</c> while the policy lives here.
/// </summary>
public static class ExceptionHandlerOptionsExtensions
{
    /// <summary>The status code Nginx uses when the client closes the connection before a response; not an IANA code, so it has no <see cref="StatusCodes"/> constant.</summary>
    private const int Status499ClientClosedRequest = 499;

    /// <summary>
    /// The default status-code policy. A <see cref="BadHttpRequestException"/> uses its own
    /// <see cref="BadHttpRequestException.StatusCode"/>; a <see cref="FluentValidation.ValidationException"/>
    /// maps to <see cref="StatusCodes.Status400BadRequest"/>; any <see cref="DomainException"/> (including
    /// its derived types — <see cref="NotFoundException"/>, <see cref="ExistException"/>,
    /// <see cref="ValidationException"/>…) uses its own <see cref="DomainException.AggregateHttpCode"/>.
    /// A handful of well-known framework exceptions map to their honest code:
    /// <see cref="UnauthorizedAccessException"/> → 403, <see cref="NotImplementedException"/> → 501,
    /// <see cref="TimeoutException"/> → 504, and <see cref="OperationCanceledException"/> (including
    /// <see cref="TaskCanceledException"/>) → 499. Anything else maps to
    /// <see cref="StatusCodes.Status500InternalServerError"/> — programming-error exceptions
    /// (<see cref="ArgumentException"/>, <see cref="InvalidOperationException"/>…) fall here on purpose,
    /// so a bug surfaces as a 500 rather than being disguised as a client error. Public so a host can
    /// compose it — map a third-party exception explicitly and delegate the rest here.
    /// </summary>
    /// <param name="exception">The unhandled exception.</param>
    /// <returns>The HTTP status code for <paramref name="exception"/>.</returns>
    public static int DefaultStatusCodeSelector(System.Exception exception) => exception switch
    {
        BadHttpRequestException e => e.StatusCode,
        FluentValidation.ValidationException => StatusCodes.Status400BadRequest,
        DomainException e => e.AggregateHttpCode,
        UnauthorizedAccessException => StatusCodes.Status403Forbidden,
        NotImplementedException => StatusCodes.Status501NotImplemented,
        TimeoutException => StatusCodes.Status504GatewayTimeout,
        OperationCanceledException => Status499ClientClosedRequest,
        _ => StatusCodes.Status500InternalServerError
    };

    /// <summary>
    /// Applies <see cref="DefaultStatusCodeSelector"/> to the options, so
    /// <c>app.UseExceptionHandler(new ExceptionHandlerOptions().WithDefaultStatusCodes())</c> maps the
    /// known exception types to their status code.
    /// <para>
    /// <b>Prerequisite:</b> an <c>IProblemDetailsService</c> must be registered — call
    /// <c>AddProblemDetailsContext()</c> (JorgeCostaMacia.Http.ProblemDetails) or plain
    /// <c>AddProblemDetails()</c>; without it the exception-handler middleware throws an
    /// <see cref="InvalidOperationException"/> when the pipeline is built.
    /// </para>
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <returns>The same <paramref name="options"/>, for chaining.</returns>
    public static ExceptionHandlerOptions WithDefaultStatusCodes(this ExceptionHandlerOptions options)
    {
        options.StatusCodeSelector = DefaultStatusCodeSelector;

        return options;
    }
}
