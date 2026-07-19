using Asp.Versioning;
using JorgeCostaMacia.Http.MinimalApi.Versioning.Infrastructure;

namespace JorgeCostaMacia.Http.MinimalApi.Versioning.Tests.Infrastructure;

public class ApiVersioningOptionsExtensionsTests
{
    [Fact]
    public void WithDefaults_ReturnsSameOptions_ForChaining()
    {
        ApiVersioningOptions options = new ApiVersioningOptions();

        Assert.Same(options, options.WithDefaults(2));
    }

    [Fact]
    public void WithDefaults_ConfiguresUrlSegmentVersioning_FromTheGivenVersion()
    {
        ApiVersioningOptions options = new ApiVersioningOptions().WithDefaults(2);

        Assert.Equal(new ApiVersion(2), options.DefaultApiVersion);
        Assert.True(options.ReportApiVersions);
        Assert.True(options.AssumeDefaultVersionWhenUnspecified);
        Assert.IsType<UrlSegmentApiVersionReader>(options.ApiVersionReader);
    }

    [Fact]
    public void WithDefaults_DefaultsToVersionOne_WhenNoVersionGiven()
        => Assert.Equal(new ApiVersion(1), new ApiVersioningOptions().WithDefaults().DefaultApiVersion);

    [Fact]
    public void WithDefaults_DefaultsToVersionOne_WhenNull()
        => Assert.Equal(new ApiVersion(1), new ApiVersioningOptions().WithDefaults(null).DefaultApiVersion);
}
