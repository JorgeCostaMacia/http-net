using Asp.Versioning.ApiExplorer;
using JorgeCostaMacia.Http.MinimalApi.Versioning.Infrastructure;

namespace JorgeCostaMacia.Http.MinimalApi.Versioning.Tests.Infrastructure;

public class ApiExplorerOptionsExtensionsTests
{
    [Fact]
    public void WithDefaultsExplorer_ReturnsSameOptions_ForChaining()
    {
        ApiExplorerOptions options = new ApiExplorerOptions();

        Assert.Same(options, options.WithDefaultsExplorer());
    }

    [Fact]
    public void WithDefaultsExplorer_ConfiguresVersionedGroupNamesAndUrlSubstitution()
    {
        ApiExplorerOptions options = new ApiExplorerOptions().WithDefaultsExplorer();

        Assert.Equal("'v'V", options.GroupNameFormat);
        Assert.True(options.SubstituteApiVersionInUrl);
    }
}
