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
    /// <summary>The group-name format <see cref="WithDefaults"/> applies when none is given: <c>'v'V</c> (e.g. <c>v1</c>).</summary>
    public const string DefaultGroupNameFormat = "'v'V";

    /// <summary>
    /// Applies the default API-explorer policy: the <paramref name="groupNameFormat"/> group-name format
    /// (default <c>'v'V</c>, e.g. <c>v1</c>) and substitution of the <c>{version:apiVersion}</c> token
    /// directly in the route URL.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="groupNameFormat">
    /// The Swagger/OpenAPI group-name format; <see langword="null"/> or empty (the default) falls back to
    /// <see cref="DefaultGroupNameFormat"/> (<c>'v'V</c>).
    /// </param>
    /// <returns>The same <paramref name="options"/>, for chaining.</returns>
    public static ApiExplorerOptions WithDefaults(this ApiExplorerOptions options, string? groupNameFormat = null)
    {
        options.GroupNameFormat = !string.IsNullOrEmpty(groupNameFormat) ? groupNameFormat : DefaultGroupNameFormat;
        options.SubstituteApiVersionInUrl = true;

        return options;
    }
}
