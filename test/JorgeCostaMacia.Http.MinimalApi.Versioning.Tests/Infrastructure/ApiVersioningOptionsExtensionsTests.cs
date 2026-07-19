using Asp.Versioning;
using JorgeCostaMacia.Http.MinimalApi.Versioning.Infrastructure;

namespace JorgeCostaMacia.Http.MinimalApi.Versioning.Tests.Infrastructure;

public class ApiVersioningOptionsExtensionsTests
{
    [Fact]
    public void WithDefaultsVersioning_ReturnsSameOptions_ForChaining()
    {
        ApiVersioningOptions options = new ApiVersioningOptions();

        Assert.Same(options, options.WithDefaultsVersioning(2));
    }

    [Fact]
    public void WithDefaultsVersioning_ConfiguresUrlSegmentVersioning_FromTheGivenVersion()
    {
        ApiVersioningOptions options = new ApiVersioningOptions().WithDefaultsVersioning(2);

        Assert.Equal(new ApiVersion(2), options.DefaultApiVersion);
        Assert.True(options.ReportApiVersions);
        Assert.True(options.AssumeDefaultVersionWhenUnspecified);
        Assert.IsType<UrlSegmentApiVersionReader>(options.ApiVersionReader);
    }
}
