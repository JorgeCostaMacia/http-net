using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JorgeCostaMacia.Http.MinimalApi.Versioning.Tests;

public class VersioningContextTests
{
    private static IConfiguration Configuration(string? apiVersion = "2")
        => new ConfigurationBuilder()
            .AddInMemoryCollection(apiVersion is null ? [] : new Dictionary<string, string?> { ["ApiVersion"] = apiVersion })
            .Build();

    [Fact]
    public void AddVersioningContext_ReturnsSameServiceCollection_ForChaining()
    {
        ServiceCollection services = new();

        Assert.Same(services, services.AddVersioningContext(Configuration()));
    }

    [Fact]
    public void AddVersioningContext_ConfiguresVersioning_FromConfiguration()
    {
        ServiceProvider provider = new ServiceCollection().AddVersioningContext(Configuration("2")).BuildServiceProvider();

        ApiVersioningOptions options = provider.GetRequiredService<IOptions<ApiVersioningOptions>>().Value;

        Assert.Equal(new ApiVersion(2), options.DefaultApiVersion);
        Assert.True(options.ReportApiVersions);
        Assert.True(options.AssumeDefaultVersionWhenUnspecified);
        Assert.IsType<UrlSegmentApiVersionReader>(options.ApiVersionReader);
    }

    [Fact]
    public void AddVersioningContext_ConfiguresApiExplorer_ForUrlSubstitution()
    {
        ServiceProvider provider = new ServiceCollection().AddVersioningContext(Configuration()).BuildServiceProvider();

        ApiExplorerOptions options = provider.GetRequiredService<IOptions<ApiExplorerOptions>>().Value;

        Assert.Equal("'v'V", options.GroupNameFormat);
        Assert.True(options.SubstituteApiVersionInUrl);
    }

    [Fact]
    public void AddVersioningContext_MissingApiVersion_Throws()
        => Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddVersioningContext(Configuration(null)));

    [Fact]
    public void AddVersioningContext_NonNumericApiVersion_Throws()
        => Assert.ThrowsAny<Exception>(() => new ServiceCollection().AddVersioningContext(Configuration("one")));
}
