using Asp.Versioning.ApiExplorer;

namespace JorgeCostaMacia.Http.MinimalApi.Versioning.Infrastructure;

/// <summary>
/// Extensions for <see cref="ApiExplorerOptions"/> that apply the default API-explorer policy for
/// URL-segment versioning, so Swagger/OpenAPI groups line up with the versioned routes. Kept as an
/// <see cref="ApiExplorerOptions"/> extension (not a hidden facade) so the <c>AddApiExplorer</c> call
/// stays visible in the host's <c>Program</c> while the policy lives here.
/// </summary>
public static class ApiExplorerOptionsExtensions
{
    /// <summary>
    /// Applies the default API-explorer policy: a <c>'v'V</c> group-name format (e.g. <c>v1</c>) and
    /// substitution of the <c>{version:apiVersion}</c> token directly in the route URL.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <returns>The same <paramref name="options"/>, for chaining.</returns>
    public static ApiExplorerOptions WithDefaults(this ApiExplorerOptions options)
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;

        return options;
    }
}
