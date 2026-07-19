using System.Text.Json;

namespace JorgeCostaMacia.Http.ProblemDetails.Infrastructure.Handlers;

/// <summary>
/// Enriches the <see cref="Microsoft.AspNetCore.Http.ProblemDetailsContext"/> for a
/// <see cref="Microsoft.AspNetCore.Http.BadHttpRequestException"/>.
/// </summary>
internal static class BadHttpRequestExceptionHandler
{
    /// <summary>
    /// Sets <c>AggregateId</c>, <c>AggregateCode</c>, and <c>AggregateType</c> to
    /// <see langword="null"/>, and adds an <c>Errors</c> extension built from the first matching
    /// cause: missing required JSON properties, an invalid value at a known JSON path, a missing
    /// request body, or an unparsable request body. All keys (and the property names referenced
    /// in error messages) are converted through the application's configured
    /// <see cref="JsonSerializerOptions.PropertyNamingPolicy"/>.
    /// </summary>
    /// <param name="ctx">The <see cref="Microsoft.AspNetCore.Http.ProblemDetailsContext"/> being customized.</param>
    /// <param name="exception">The <see cref="Microsoft.AspNetCore.Http.BadHttpRequestException"/> being handled.</param>
    /// <param name="namingPolicy">The naming policy used to convert field names.</param>
    public static void Handle(Microsoft.AspNetCore.Http.ProblemDetailsContext ctx, Microsoft.AspNetCore.Http.BadHttpRequestException exception, JsonNamingPolicy namingPolicy)
    {
        ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("AggregateId")] = null;
        ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("AggregateCode")] = null;
        ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("AggregateType")] = null;

        if (exception.InnerException is JsonException jsonExceptionMissing && jsonExceptionMissing.Message.Contains("missing required properties"))
        {
            HandleMissingProperties(ctx, jsonExceptionMissing, namingPolicy);
        }
        else if (exception.InnerException is JsonException jsonExceptionInvalid && !string.IsNullOrEmpty(jsonExceptionInvalid.Path))
        {
            HandleInvalidProperties(ctx, jsonExceptionInvalid, namingPolicy);
        }
        else if ((exception.Message.Contains("Required parameter") && exception.Message.Contains("was not provided from body."))
            || exception.Message.Contains("no body was provided"))
        {
            ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("Errors")] = new Dictionary<string, string[]>
            {
                { namingPolicy.ConvertName("Request"), new string[] { "A non-empty request body is required." } }
            };
        }
        else if (exception.Message.Contains("Failed to read parameter") && exception.Message.Contains("from the request body as JSON."))
        {
            ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("Errors")] = new Dictionary<string, string[]>
            {
                { namingPolicy.ConvertName("Request"), new string[] { "One or more fields have an invalid data type format." } }
            };
        }
        else
        {
            // an unrecognized wording (another runtime version / future rewording): keep the field
            // present — and null — so clients can rely on it, matching the DomainException contract.
            ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("Errors")] = null;
        }
    }

    /// <summary>
    /// Builds the <c>Errors</c> extension from the <see cref="JsonException"/>'s message,
    /// listing each missing required property, or falling back to a generic message if no
    /// property names could be extracted.
    /// </summary>
    /// <param name="ctx">The <see cref="Microsoft.AspNetCore.Http.ProblemDetailsContext"/> being customized.</param>
    /// <param name="exception">The inner <see cref="JsonException"/> reporting the missing properties.</param>
    /// <param name="namingPolicy">The naming policy used to convert field names.</param>
    private static void HandleMissingProperties(Microsoft.AspNetCore.Http.ProblemDetailsContext ctx, JsonException exception, JsonNamingPolicy namingPolicy)
    {
        List<string> missings = System.Text.RegularExpressions.Regex
            .Matches(exception.Message, @"'([^']*)'|\[([^\]]*)\]")
            .Select(m => m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value)
            .Where(f => !string.IsNullOrWhiteSpace(f) && f != "request" && !f.Contains('.'))
            .ToList();

        if (missings.Any())
        {
            ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("Errors")] = missings.ToDictionary(
                field => namingPolicy.ConvertName(field),
                field => new string[] { $"'{namingPolicy.ConvertName(field)}' must be present." }
            );
        }
        else
        {
            ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("Errors")] = new Dictionary<string, string[]>
            {
                { namingPolicy.ConvertName("Request"), new string[] { "One or more required fields are not present." } }
            };
        }
    }

    /// <summary>
    /// Builds the <c>Errors</c> extension from the <see cref="JsonException"/>'s
    /// <see cref="JsonException.Path"/>, identifying the field with the invalid value, or
    /// falling back to a generic message if the field name could not be determined.
    /// </summary>
    /// <param name="ctx">The <see cref="Microsoft.AspNetCore.Http.ProblemDetailsContext"/> being customized.</param>
    /// <param name="exception">The inner <see cref="JsonException"/> reporting the invalid path.</param>
    /// <param name="namingPolicy">The naming policy used to convert field names.</param>
    private static void HandleInvalidProperties(Microsoft.AspNetCore.Http.ProblemDetailsContext ctx, JsonException exception, JsonNamingPolicy namingPolicy)
    {
        string? fieldRaw = exception.Path?.Split('.').LastOrDefault()?.TrimStart('$');

        if (!string.IsNullOrEmpty(fieldRaw))
        {
            string field = namingPolicy.ConvertName(fieldRaw);

            ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("Errors")] = new Dictionary<string, string[]>
            {
                { field, new string[] { $"'{field}' has an invalid data type format." } }
            };
        }
        else
        {
            ctx.ProblemDetails.Extensions[namingPolicy.ConvertName("Errors")] = new Dictionary<string, string[]>
            {
                { namingPolicy.ConvertName("Request"), new string[] { "One or more fields have an invalid data type format." } }
            };
        }
    }
}
