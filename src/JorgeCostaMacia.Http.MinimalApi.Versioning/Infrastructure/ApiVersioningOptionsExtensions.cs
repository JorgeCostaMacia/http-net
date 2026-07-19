using Asp.Versioning;

namespace JorgeCostaMacia.Http.MinimalApi.Versioning.Infrastructure;

/// <summary>
/// Extensions for <see cref="ApiVersioningOptions"/> that apply the default versioning policy:
/// URL-segment versioning (e.g. <c>/v1/resource</c>) with a configured default version. Kept as an
/// <see cref="ApiVersioningOptions"/> extension (not a hidden <c>Add…</c> facade) so the
/// <c>AddApiVersioning</c> call stays visible in the host's <c>Program</c> while the policy lives here.
/// </summary>
public static class ApiVersioningOptionsExtensions
{
    /// <summary>
    /// Applies the default versioning policy: <paramref name="apiVersion"/> as the default version
    /// (the one assumed when a request does not specify one — not the only supported version; endpoints
    /// still declare their own with <c>HasApiVersion</c>), version headers reported
    /// (<c>api-supported-versions</c>), the default version assumed when the request does not specify one,
    /// and the version read from a URL segment (<see cref="UrlSegmentApiVersionReader"/>).
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="apiVersion">The major API version to assume as the default; defaults to <c>1</c>.</param>
    /// <returns>The same <paramref name="options"/>, for chaining.</returns>
    public static ApiVersioningOptions WithDefaults(this ApiVersioningOptions options, int apiVersion = 1)
    {
        options.DefaultApiVersion = new ApiVersion(apiVersion);
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();

        return options;
    }
}
