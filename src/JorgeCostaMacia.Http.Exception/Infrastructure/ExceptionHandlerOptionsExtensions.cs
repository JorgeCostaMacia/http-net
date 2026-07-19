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
    /// <summary>
    /// The default status-code policy, our own domain exceptions first, then framework and third-party.
    /// Any <see cref="DomainException"/> (including its derived types — <see cref="NotFoundException"/>,
    /// <see cref="ExistException"/>, <see cref="ValidationException"/>…) uses its own
    /// <see cref="DomainException.AggregateHttpCode"/>; a <see cref="BadHttpRequestException"/> uses its own
    /// <see cref="BadHttpRequestException.StatusCode"/>; a <see cref="FluentValidation.ValidationException"/>
    /// maps to <see cref="StatusCodes.Status400BadRequest"/>; and a handful of framework/BCL exceptions map
    /// to their honest code: <see cref="UnauthorizedAccessException"/> → 403,
    /// <see cref="OperationCanceledException"/> (including <see cref="TaskCanceledException"/>) → 499, and
    /// <see cref="NotImplementedException"/> → 501. Anything else maps to
    /// <see cref="StatusCodes.Status500InternalServerError"/> — this deliberately includes failures that
    /// depend on an upstream service (a timed-out or unreachable dependency), so the response never leaks
    /// through a gateway code (502/503/504) that a downstream even exists; and programming-error exceptions
    /// (<see cref="ArgumentException"/>, <see cref="InvalidOperationException"/>…) fall here too, so a bug
    /// surfaces as a 500 rather than being disguised as a client error. Public so a host can compose it —
    /// map a third-party exception explicitly and delegate the rest here.
    /// </summary>
    /// <param name="exception">The unhandled exception.</param>
    /// <returns>The HTTP status code for <paramref name="exception"/>.</returns>
    public static int DefaultStatusCodeSelector(System.Exception exception) => exception switch
    {
        DomainException e => e.AggregateHttpCode,
        BadHttpRequestException e => e.StatusCode,
        FluentValidation.ValidationException => StatusCodes.Status400BadRequest,
        UnauthorizedAccessException => StatusCodes.Status403Forbidden,
        OperationCanceledException => StatusCodes.Status499ClientClosedRequest,
        NotImplementedException => StatusCodes.Status501NotImplemented,
        _ => StatusCodes.Status500InternalServerError
    };

    /// <summary>
    /// Applies <see cref="DefaultStatusCodeSelector"/> to the options, so
    /// <c>app.UseExceptionHandler(new ExceptionHandlerOptions().WithDefaultStatusCodes())</c> maps the
    /// known exception types to their status code.
    /// <para>
    /// <b>Prerequisite:</b> an <c>IProblemDetailsService</c> must be registered — call
    /// <c>AddProblemDetails(o =&gt; o.WithDefaults())</c>
    /// (JorgeCostaMacia.Http.ProblemDetails) or plain <c>AddProblemDetails()</c>; without it the
    /// exception-handler middleware throws an <see cref="InvalidOperationException"/> when the pipeline is built.
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
