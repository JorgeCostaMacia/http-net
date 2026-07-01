using JorgeCostaMacia.Exception.Domain;
using System.Text.Json;

namespace JorgeCostaMacia.Http.ProblemDetails.Infrastructure;

/// <summary>
/// Enriches the <see cref="Microsoft.AspNetCore.Http.ProblemDetailsContext"/> for a
/// <see cref="DomainException"/> with aggregate metadata and, for validation failures, a
/// per-field error dictionary.
/// </summary>
internal static class DomainExceptionHandler
{
    /// <summary>
    /// Adds <c>AggregateId</c>, <c>AggregateCode</c>, and <c>AggregateType</c> extensions to the
    /// <see cref="Microsoft.AspNetCore.Http.ProblemDetailsContext.ProblemDetails"/>, converting
    /// every key (and, for a <see cref="ValidationException"/>, the property names referenced in
    /// each error message) through <paramref name="namingPolicy"/>, so error responses keep the
    /// same casing as the rest of the API's JSON.
    /// <para>
    /// For a <see cref="ValidationException"/>, also adds an <c>Errors</c> extension grouping
    /// <see cref="FluentValidation.Results.ValidationFailure"/> messages by property name. For
    /// any other <see cref="DomainException"/>, <c>Errors</c> is set to <see langword="null"/>,
    /// keeping the field present for consistent client-side handling.
    /// </para>
    /// </summary>
    /// <param name="ctx">The <see cref="Microsoft.AspNetCore.Http.ProblemDetailsContext"/> being customized.</param>
    /// <param name="exception">The <see cref="DomainException"/> being handled.</param>
    /// <param name="namingPolicy">The naming policy used to convert extension keys and field names.</param>
    public static void Handle(Microsoft.AspNetCore.Http.ProblemDetailsContext ctx, DomainException exception, JsonNamingPolicy namingPolicy)
    {
        ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("AggregateId")] = exception.AggregateId;
        ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("AggregateCode")] = exception.AggregateCode;
        ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("AggregateType")] = namingPolicy.ConvertName(exception.AggregateType.Split('.').Last());

        if (exception is ValidationException ex)
        {
            ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("Errors")] = ex.Validations
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    e => namingPolicy.ConvertName(e.Key),
                    e => e.Select(x => x.ErrorMessage.Replace($"'{x.PropertyName}'", $"'{namingPolicy.ConvertName(x.PropertyName)}'")).ToArray()
                );
        }
        else
        {
            ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("Errors")] = null;
        }
    }
}
