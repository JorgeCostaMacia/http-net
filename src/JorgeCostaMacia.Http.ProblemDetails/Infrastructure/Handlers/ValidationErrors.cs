using System.Text.Json;
using FluentValidation.Results;

namespace JorgeCostaMacia.Http.ProblemDetails.Infrastructure.Handlers;

/// <summary>
/// Builds the ProblemDetails <c>Errors</c> extension from a set of <see cref="ValidationFailure"/>,
/// grouping messages by property name and converting every key (and the property names quoted inside
/// each message) through the application's naming policy — so validation responses keep the same
/// casing as the rest of the API's JSON.
/// </summary>
internal static class ValidationErrors
{
    /// <summary>
    /// Groups <paramref name="failures"/> by property name (after applying <paramref name="namingPolicy"/>,
    /// so two names that collide under the policy — e.g. <c>Id</c>/<c>ID</c> → <c>id</c> — merge into one
    /// entry instead of throwing a duplicate-key exception) into a field → messages dictionary.
    /// </summary>
    /// <param name="failures">The validation failures to render.</param>
    /// <param name="namingPolicy">The naming policy used to convert field names.</param>
    /// <returns>A dictionary mapping each (policy-converted) property name to its error messages.</returns>
    public static Dictionary<string, string[]> Build(IEnumerable<ValidationFailure> failures, JsonNamingPolicy namingPolicy)
        => failures
            .GroupBy(failure => namingPolicy.ConvertName(failure.PropertyName))
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage.Replace($"'{failure.PropertyName}'", $"'{namingPolicy.ConvertName(failure.PropertyName)}'")).ToArray()
            );
}
