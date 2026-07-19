using System.Text.Json;

namespace JorgeCostaMacia.Http.ProblemDetails.Infrastructure.Handlers;

/// <summary>
/// Enriches the <see cref="Microsoft.AspNetCore.Http.ProblemDetailsContext"/> for a raw
/// <see cref="FluentValidation.ValidationException"/> — one thrown directly, not wrapped in a domain
/// <c>ValidationException</c>. Sets the aggregate metadata to <see langword="null"/> (it carries none)
/// and builds a per-field <c>Errors</c> dictionary from its failures, so a 400 originating in
/// FluentValidation returns the same body shape as a domain validation failure.
/// </summary>
internal static class FluentValidationExceptionHandler
{
    /// <summary>
    /// Sets <c>AggregateId</c>, <c>AggregateCode</c>, and <c>AggregateType</c> to <see langword="null"/>
    /// and adds an <c>Errors</c> extension grouping the exception's failures by property name.
    /// </summary>
    /// <param name="ctx">The <see cref="Microsoft.AspNetCore.Http.ProblemDetailsContext"/> being customized.</param>
    /// <param name="exception">The <see cref="FluentValidation.ValidationException"/> being handled.</param>
    /// <param name="namingPolicy">The naming policy used to convert extension keys and field names.</param>
    public static void Handle(Microsoft.AspNetCore.Http.ProblemDetailsContext ctx, FluentValidation.ValidationException exception, JsonNamingPolicy namingPolicy)
    {
        ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("AggregateId")] = null;
        ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("AggregateCode")] = null;
        ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("AggregateType")] = null;
        ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("Errors")] = ValidationErrors.Build(exception.Errors, namingPolicy);
    }
}
