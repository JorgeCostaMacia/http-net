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
    /// <summary>The major API version <see cref="WithDefaults"/> assumes when no major is given (or it is <see langword="null"/>).</summary>
    public const int DefaultMajorVersion = 1;

    /// <summary>
    /// Applies the default versioning policy: <paramref name="majorVersion"/>.<paramref name="minorVersion"/>
    /// as the default version (the one assumed when a request does not specify one — not the only supported
    /// version; endpoints still declare their own with <c>HasApiVersion</c>), version headers reported
    /// (<c>api-supported-versions</c>), the default version assumed when the request does not specify one,
    /// and the version read from a URL segment (<see cref="UrlSegmentApiVersionReader"/>).
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="majorVersion">
    /// The major version to assume as the default; <see langword="null"/> (or omitted) falls back to
    /// <see cref="DefaultMajorVersion"/> (<c>1</c>), so a nullable
    /// <c>IConfiguration.GetValue&lt;int?&gt;("ApiVersion")</c> can be passed straight in.
    /// </param>
    /// <param name="minorVersion">
    /// The minor version to assume as the default; <see langword="null"/> (the default) means major-only
    /// (e.g. <c>v1</c> rather than <c>v1.0</c>).
    /// </param>
    /// <returns>The same <paramref name="options"/>, for chaining.</returns>
    public static ApiVersioningOptions WithDefaults(this ApiVersioningOptions options, int? majorVersion = null, int? minorVersion = null)
    {
        // pick the constructor by whether a minor was given: with one the version is major.minor (v1.1);
        // without one it stays major-only (v1) — the two-arg ctor with a 0 minor would render as v1.0.
        options.DefaultApiVersion = minorVersion is int
             ? new ApiVersion(majorVersion ?? DefaultMajorVersion, minorVersion)
             : new ApiVersion(majorVersion ?? DefaultMajorVersion);
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();

        return options;
    }
}
