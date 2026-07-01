using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using JorgeCostaMacia.Exception.Domain;

namespace JorgeCostaMacia.Http.Exception;

/// <summary>
/// Configures the HTTP exception handling pipeline for a <see cref="WebApplication"/>, mapping
/// known exception types to the HTTP status code returned to the client.
/// </summary>
public static class ExceptionContext
{
    /// <summary>
    /// Registers the global exception handler middleware and defines the status code selection
    /// logic used when an unhandled exception reaches it.
    /// <para>
    /// <b>Status code mapping:</b> a <see cref="BadHttpRequestException"/> uses its own
    /// <see cref="BadHttpRequestException.StatusCode"/>; a <see cref="FluentValidation.ValidationException"/>
    /// maps to <see cref="StatusCodes.Status400BadRequest"/>; any <see cref="DomainException"/>
    /// (including its derived types) uses its own <see cref="DomainException.AggregateHttpCode"/>;
    /// any other exception maps to <see cref="StatusCodes.Status500InternalServerError"/>.
    /// </para>
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The same <see cref="WebApplication"/> instance, to allow method chaining.</returns>
    public static WebApplication UseExceptionContext(this WebApplication app)
    {
        app.UseExceptionHandler(new ExceptionHandlerOptions()
        {
            StatusCodeSelector = ex => ex switch
            {
                BadHttpRequestException e => e.StatusCode,
                FluentValidation.ValidationException => StatusCodes.Status400BadRequest,
                DomainException e => e.AggregateHttpCode,
                _ => StatusCodes.Status500InternalServerError
            }
        });

        return app;
    }
}
