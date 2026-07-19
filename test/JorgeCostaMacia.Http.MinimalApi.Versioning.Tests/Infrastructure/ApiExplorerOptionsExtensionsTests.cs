using Asp.Versioning.ApiExplorer;
using JorgeCostaMacia.Http.MinimalApi.Versioning.Infrastructure;

namespace JorgeCostaMacia.Http.MinimalApi.Versioning.Tests.Infrastructure;

public class ApiExplorerOptionsExtensionsTests
{
    [Fact]
    public void WithDefaults_ReturnsSameOptions_ForChaining()
    {
        ApiExplorerOptions options = new ApiExplorerOptions();

        Assert.Same(options, options.WithDefaults());
    }

    [Fact]
    public void WithDefaults_ConfiguresVersionedGroupNamesAndUrlSubstitution()
    {
        ApiExplorerOptions options = new ApiExplorerOptions().WithDefaults();

        Assert.Equal("'v'V", options.GroupNameFormat);
        Assert.True(options.SubstituteApiVersionInUrl);
    }
}
